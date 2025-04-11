using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Flagrum.Abstractions;
using Flagrum.Abstractions.AssetExplorer;
using Flagrum.Core.Entities.Xml2;
using Flagrum.Core.Graphics.Textures.Luminous;
using Flagrum.Generators;
using Flagrum.Application.Features.AssetExplorer.Data;
using Flagrum.Application.Features.ModManager.Data;
using Flagrum.Application.Features.ModManager.Instructions;
using Flagrum.Application.Features.ModManager.Instructions.Abstractions;
using Flagrum.Application.Features.Shared;
using Flagrum.Application.Services;
using Injectio.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Flagrum.Application.Features.ModManager.Services;

/// <summary>
/// Converts standard assets to Luminous file formats.
/// </summary>
[RegisterScoped]
public partial class AssetConverter
{
    [Inject] private readonly IProfileService _profile;
    [Inject] private readonly TextureConverter _textureConverter;

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

    private ConcurrentDictionary<string, BlackTexture> _textureMetadata;

    public AssetConverter(TextureConverter textureConverter) => _textureConverter = textureConverter;

    /// <summary>
    /// Stores texture metadata so it can be referred to in <see cref="ConvertImageToReplacementBtex" />.
    /// </summary>
    /// <param name="metadata">
    /// The metadata to store.<br />
    /// <b>Key:</b> The URI of the texture.
    /// <b>Value:</b> The BTEX header of the original texture file matching the key URI.
    /// </param>
    public void SetTextureMetadata(ConcurrentDictionary<string, BlackTexture> metadata)
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
        var texture = _textureMetadata[file.Uri].ImageData[0];
        var extension = file.FilePath[(file.FilePath.LastIndexOf('.') + 1)..].ToLower();

        if (file.Uri.Split('/')[^1].Split('.')[0].EndsWith("_mrgb"))
        {
            texture.Format = BlackTexturePixelFormat.A8B8G8R8;
        }

        return _textureConverter.ToBtex(new TextureConversionRequest
        {
            Name = texture.FileName,
            PixelFormat = texture.Format,
            ImageFlags = texture.ImageFlags,
            MipLevels = texture.MipCount,
            SourceFormat = extension switch
            {
                "tga" => TextureSourceFormat.Targa,
                "dds" => TextureSourceFormat.Dds,
                _ => TextureSourceFormat.Wic
            },
            SourceData = data,
            Game = _profile.Current.Type
        }).Write();
    }

    /// <summary>
    /// Converts an image file of any supported file format to a BTEX file for use in Luminous.
    /// This method should be used when adding new textures so that the appropriate attributes
    /// can be calculated based on the file name and image metadata.
    /// </summary>
    /// <param name="file">The build instruction pertaining to the new texture.</param>
    /// <param name="data">A buffer containing the unconverted image file.</param>
    /// <returns>A buffer containing the convertex BTEX file.</returns>
    /// <remarks>
    /// This is not a very reliable method. The desired pixel format is determined purely based on the name of
    /// the image file, and other assumptions about the texture are also made on a similar basis.
    /// TODO: Provide an interface where the mod author can specify texture properties explicitly.
    /// </remarks>
    private byte[] ConvertImageToNewBtex(PackedAssetBuildInstruction file, byte[] data)
    {
        var originalName = file.Uri.Split('/').Last();
        var originalNameWithoutExtension = originalName[..originalName.LastIndexOf('.')];
        var extension = file.FilePath[(file.FilePath.LastIndexOf('.') + 1)..].ToLower();

        var type = TextureType.BaseColor;
        if (file is not AddToPackedTextureArrayBuildInstruction)
        {
            foreach (var (suffix, textureType) in _textureTypes)
            {
                if (originalNameWithoutExtension.EndsWith(suffix) ||
                    originalNameWithoutExtension.EndsWith(suffix + "_$h"))
                {
                    type = textureType;
                    break;
                }
            }
        }

        return _textureConverter.ConvertImageToNewBtex(originalNameWithoutExtension, extension, type, data,
            _profile.Current.Type);
    }
}