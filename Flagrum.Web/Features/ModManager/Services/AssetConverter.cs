using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Flagrum.Core.Ebex.Xmb2;
using Flagrum.Core.Gfxbin.Btex;
using Flagrum.Core.Utilities.Types;
using Flagrum.Web.Features.AssetExplorer.Data;
using Flagrum.Web.Persistence.Entities.ModManager;
using Flagrum.Web.Services;

namespace Flagrum.Web.Features.ModManager.Services;

public class AssetConverter
{
    private readonly ProfileService _profile;

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

    private ConcurrentDictionary<string, BtexHeader> _textureMetadata;

    public AssetConverter(ProfileService profile)
    {
        _profile = profile;
    }

    public void SetTextureMetadata(ConcurrentDictionary<string, BtexHeader> metadata)
    {
        _textureMetadata = metadata;
    }

    public byte[] Convert(EarcModFile file)
    {
        var sourceData = File.ReadAllBytes(file.ReplacementFilePath);
        var type = AssetExplorerItem.GetType(file.Uri);

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (type)
        {
            case ExplorerItemType.Texture when !file.ReplacementFilePath.EndsWith(".btex", StringComparison.OrdinalIgnoreCase):
                return file.Type == EarcFileChangeType.Replace 
                    ? ConvertImageToReplacementBtex(file, sourceData) 
                    : ConvertImageToNewBtex(file, sourceData);
            case ExplorerItemType.Xml:
                var tag = Encoding.UTF8.GetString(sourceData[..4]);
                if (tag != "XMB2")
                {
                    try
                    {
                        return new Xmb2Writer(sourceData).Write();
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

    private byte[] ConvertImageToReplacementBtex(EarcModFile file, byte[] data)
    {
        var converter = new TextureConverter(_profile.Current.Type);
        var header = _textureMetadata[file.Uri];
        var extension = file.ReplacementFilePath[(file.ReplacementFilePath.LastIndexOf('.') + 1)..].ToLower();
        
        return converter.ToBtex(new BtexBuildRequest
        {
            Name = header.p_Name,
            PixelFormat = header.Format,
            ImageFlags = header.p_ImageFlags,
            MipLevels = header.MipMapCount,
            SourceFormat = extension switch
            {
                "tga" => BtexSourceFormat.Targa,
                "dds" => BtexSourceFormat.Dds,
                _ => BtexSourceFormat.Wic
            },
            SourceData = data,
            AddSedbHeader = _profile.Current.Type == LuminousGame.FFXV
        });
    }
    
    private byte[] ConvertImageToNewBtex(EarcModFile file, byte[] data)
    {
        var originalName = file.Uri.Split('/').Last();
        var originalNameWithoutExtension = originalName[..originalName.LastIndexOf('.')];
        var extension = file.ReplacementFilePath[(file.ReplacementFilePath.LastIndexOf('.') + 1)..].ToLower();

        var type = TextureType.BaseColor;
        if (file.Type != EarcFileChangeType.AddToTextureArray)
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
        
        var converter = new TextureConverter(_profile.Current.Type);
        return converter.ToBtex(originalNameWithoutExtension, extension, type, data);
    }
}