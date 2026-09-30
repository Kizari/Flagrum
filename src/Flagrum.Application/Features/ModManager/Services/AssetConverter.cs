using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Flagrum.Abstractions.AssetExplorer;
using Flagrum.Application.Features.AssetExplorer.Data;
using Flagrum.Application.Features.ModManager.Data;
using Flagrum.Application.Features.ModManager.Instructions;
using Flagrum.Application.Features.ModManager.Instructions.Abstractions;
using Flagrum.Core.Entities.Xml2;
using Flagrum.Core.Graphics.Textures.DirectX;
using Flagrum.Core.Graphics.Textures.Luminous;
using Flagrum.Core.Graphics.Textures.Luminous.Builder;
using Flagrum.Core.Graphics.Textures.Luminous.DataSources;
using Injectio.Attributes;
using SixLabors.ImageSharp;

namespace Flagrum.Application.Features.ModManager.Services;

/// <summary>
/// Converts standard assets to Luminous file formats.
/// </summary>
[RegisterScoped<AssetConverter>]
public partial class AssetConverter
{
    /// <summary>
    /// Maps texture filename suffixes to the respective <see cref="TextureType" />.
    /// </summary>
    private readonly ConcurrentDictionary<string, TextureType> _textureTypes = new(new Dictionary<string, TextureType>
    {
        {"_mrs", TextureType.Mrs},
        {"_n", TextureType.Normal},
        {"_a", TextureType.Opacity},
        {"_o", TextureType.AmbientOcclusion},
        {"_mro", TextureType.BaseColor},
        {"_hro", TextureType.BaseColor},
        {"_b", TextureType.BaseColor},
        {"_ba", TextureType.BaseColor},
        {"_e", TextureType.BaseColor},
        {"_mrgb", TextureType.MenuItem}
    });

    private ConcurrentDictionary<string, ExistingTextureMetadata> _textureMetadata;

    public static HashSet<string> SrgbSuffixes { get; } =
    [
        "_b", "_b01", "_b1", "_b2", "_b3", "_b4", "_b6", "_b7", "_ba", "_ba1", "_ba2",
        "_co", "_d", "_fa", "_fa2", "_g", "_i", "_k", "_l", "_p", "_q", "_rc", "_sss",
        "_t", "_u", "_v", "_va", "_vc", "_vfx", "_w"
    ];

    /// <summary>
    /// Stores texture metadata so it can be referred to in <see cref="ConvertImageToReplacementBtex" />.
    /// </summary>
    /// <param name="metadata">
    /// The metadata to store.<br />
    /// <b>Key:</b> The URI of the texture.
    /// <b>Value:</b> The BTEX header of the original texture file matching the key URI.
    /// </param>
    public void SetTextureMetadata(ConcurrentDictionary<string, ExistingTextureMetadata> metadata)
    {
        _textureMetadata = metadata;
    }

    /// <summary>
    /// Converts a mod asset into its Luminous equivalent.<br />
    /// Returns the original asset if no suitable conversion is found.
    /// </summary>
    /// <param name="file">The build instruction pertaining to the unconverted asset.</param>
    /// <returns>A buffer containing the converted asset.</returns>
    /// <remarks>
    /// <b>Images</b> of supported types (PNG, TGA, DDS) are converted to BTEX.<br />
    /// <b>XML</b> is converted to XMB2.<br />
    /// Other conversions are not supported at this time.
    /// </remarks>
    public byte[] Convert(PackedAssetBuildInstruction file)
    {
        var sourceData = File.ReadAllBytes(file.FilePath);
        var type = AssetExplorerItem.GetType(file.Uri);

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (type)
        {
            case ExplorerItemType.Texture
                when !file.FilePath.EndsWith(".btex", StringComparison.OrdinalIgnoreCase):
                return file is ReplacePackedFileBuildInstruction
                    ? ConvertImageToReplacementBtex(file, sourceData)
                    : ConvertImageToNewBtex(file, sourceData);
            case ExplorerItemType.Xml:
                var tag = Encoding.UTF8.GetString(sourceData[..4]);
                if (tag != "XMB2")
                {
                    try
                    {
                        return new XmlBinary2Writer(sourceData).Write();
                    }
                    catch
                    {
                        // Luminous can read raw XML, so if conversion fails, just pass the XML back
                        return sourceData;
                    }
                }

                break;
        }

        return sourceData;
    }

    /// <summary>
    /// Converts an image file of any supported file format to a BTEX file for use in Luminous.
    /// This method should be used when replacing existing textures so that various attributes
    /// can be pulled from the existing texture and applied to the replacement.
    /// </summary>
    /// <param name="file">The build instruction pertaining to the texture replacement.</param>
    /// <param name="data">A buffer containing the unconverted image file.</param>
    /// <returns>A buffer containing the converted BTEX file.</returns>
    /// <remarks>
    /// Due to issues with texture files with the <c>_mrgb</c> suffix, these are forcibly set to
    /// use the <see cref="BlackTexturePixelFormat.A8B8G8R8" /> pixel format. It is not currently
    /// known if this works out well in all cases. Unfortunately this was not commented at the time
    /// of authoring, and the author does not remember what the complication was, why the fix was
    /// needed, or why the fix works. Further investigation may be warranted.
    /// </remarks>
    private byte[] ConvertImageToReplacementBtex(PackedAssetBuildInstruction file, byte[] data)
    {
        // TODO: Warn user if replacing cubemap with non-cubemap or array with non-array

        var texture = _textureMetadata[file.Uri];

        if (file.Uri.Split('/')[^1].Split('.')[0].EndsWith("_mrgb"))
        {
            texture.Format = BlackTexturePixelFormat.A8B8G8R8;
        }

        return CreateTextureBuilder(texture.Name, data, out var width, out var height,
                texture.Format, texture.Flags.HasFlag(BlackTextureImageFlags.SRGB), texture.MipCount)
            .AddRasterImage(new RasterImageDataSource(data, width, height, BlackTexturePixelFormat.R8G8B8A8_UNORM))
            .Build();
    }

    /// <summary>
    /// Converts an image file of any supported file format to a BTEX file for use in Luminous.
    /// This method should be used when adding new textures so that the appropriate attributes
    /// can be calculated based on the file name and image metadata.
    /// </summary>
    /// <param name="file">The build instruction pertaining to the new texture.</param>
    /// <param name="data">A buffer containing the unconverted image file.</param>
    /// <returns>A buffer containing the converted BTEX file.</returns>
    /// <remarks>
    /// This is not a very reliable method. The desired pixel format is determined purely based on the name of
    /// the image file, and other assumptions about the texture are also made on a similar basis.
    /// TODO: Provide an interface where the mod author can specify texture properties explicitly.
    /// </remarks>
    private byte[] ConvertImageToNewBtex(PackedAssetBuildInstruction file, byte[] data)
    {
        var originalName = file.Uri.Split('/').Last();
        var originalNameWithoutExtension = originalName[..originalName.LastIndexOf('.')];

        var suffix = originalNameWithoutExtension.TrimEnd("_$h").ToString();
        var startIndex = suffix.LastIndexOf('_');
        suffix = suffix[startIndex..];

        var type = TextureType.BaseColor;
        if (file is not AddToPackedTextureArrayBuildInstruction)
        {
            _textureTypes.TryGetValue(suffix, out type);
        }

        // TODO: Add EXR support
        return CreateTextureBuilder(originalNameWithoutExtension, data, out var width, out var height, type switch
            {
                TextureType.Normal => BlackTexturePixelFormat.BC5_UNORM,
                TextureType.AmbientOcclusion or TextureType.Opacity => BlackTexturePixelFormat.BC4_UNORM,
                TextureType.MenuSprites => BlackTexturePixelFormat.B8G8R8A8_UNORM,
                _ => BlackTexturePixelFormat.BC1_UNORM
            }, SrgbSuffixes.Contains(suffix))
            .AddRasterImage(new RasterImageDataSource(data, width, height, BlackTexturePixelFormat.R8G8B8A8_UNORM))
            .Build();
    }

    private BlackTextureBuilder CreateTextureBuilder(
        string name,
        byte[] sourceImage,
        out int width,
        out int height,
        BlackTexturePixelFormat format,
        bool isSrgb,
        int? mipMapCount = null)
    {
        if (BitConverter.ToUInt32(sourceImage, 0) == DirectDrawSurfaceHeader.MagicValue)
        {
            var span = new Span<byte>(sourceImage);
            var dds = MemoryMarshal.Read<DirectDrawSurfaceHeader>(span);
            width = (int)dds.Width;
            height = (int)dds.Height;
            mipMapCount ??= (int)dds.MipMapCount;
        }
        else
        {
            var info = Image.Identify(sourceImage);
            width = info.Width;
            height = info.Height;
            mipMapCount ??= 0; // Generate all mip levels
        }

        return new BlackTextureBuilder(name, width, height, mipMapCount.Value, format, isSrgb);
    }
}