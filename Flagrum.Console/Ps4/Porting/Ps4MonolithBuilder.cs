using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;

namespace Flagrum.Console.Ps4.Porting;

public class Ps4MonolithBuilder
{
    private const string StagingDirectory = @"C:\Modding\Chocomog\Staging";

    public void Run()
    {
        const string directory = $@"{StagingDirectory}\PortedAssets";
        const string rootEbex = "data://level/dlc_ex/mog/area_ravettrice_mog.ebex";
        
        var hashTable = new Dictionary<ulong, string>();
        using var context = Ps4Utilities.NewContext();
        
        var rootPacker = new Packer();

        foreach (var asset in context.Ps4AssetUris)
        {
            hashTable.TryAdd(Cryptography.HashFileUri64(asset.Uri), asset.Uri);
        }

        var fileCounter = 0;
        var earcCounter = 1;
        var currentPacker = new Packer();
        
        foreach (var fileName in Directory.EnumerateFiles(directory))
        {
            var tokens = fileName.Split('\\').Last().Split('.')[0].Split('_');
        
            var asset = new PortedAsset
            {
                Hash = ulong.Parse(tokens[0]),
                RelativePath = tokens[1].FromBase64(),
                OriginalSize = uint.Parse(tokens[2]),
                Flags = int.Parse(tokens[3]),
                LocalisationType = byte.Parse(tokens[4]),
                Locale = byte.Parse(tokens[5]),
                Key = ushort.Parse(tokens[6])
            };

            var uri = hashTable[asset.Hash];

            if (uri.EndsWith(".ebex") || uri.EndsWith(".prefab"))
            {
                asset.Flags = (int)ArchiveFileFlag.Autoload;
            }
            
            if (uri == rootEbex)
            {
                rootPacker.AddFileFromBackup(uri, asset.RelativePath, asset.OriginalSize, (ArchiveFileFlag)asset.Flags, asset.LocalisationType, asset.Locale, asset.Key, File.ReadAllBytes(fileName));
            }
            else
            {
                currentPacker.AddFileFromBackup(uri, asset.RelativePath, asset.OriginalSize, (ArchiveFileFlag)asset.Flags, asset.LocalisationType, asset.Locale, asset.Key, File.ReadAllBytes(fileName));
                fileCounter++;
                
                if (fileCounter >= 1000)
                {
                    fileCounter = 0;
                    currentPacker.WriteToFile($@"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas\level\dlc_ex\mog\data_{earcCounter}.earc");
                    rootPacker.AddReference($"data://level/dlc_ex/mog/data_{earcCounter}.ebex@", true);
                    earcCounter++;
                    currentPacker = new Packer();
                }
            }
        }
        
        rootPacker.WriteToFile(@"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas\level\dlc_ex\mog\area_ravettrice_mog.earc");
    }
}
//$"{uriHash}_{relativePathBase64}_{originalSize}_{flags}_{localisationType}_{locale}_{key}{file.RelativePath[file.RelativePath.LastIndexOf('.')..]}";
public class PortedAsset
{
    public ulong Hash { get; set; }
    public string RelativePath { get; set; }
    public uint OriginalSize { get; set; }
    public int Flags { get; set; }
    public byte LocalisationType { get; set; }
    public byte Locale { get; set; }
    public ushort Key { get; set; }
}