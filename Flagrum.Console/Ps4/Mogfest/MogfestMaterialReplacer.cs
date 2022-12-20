using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flagrum.Console.Ps4.Mogfest.Utilities;
using Flagrum.Core.Archive;
using Flagrum.Core.Gfxbin.Gmtl;
using Flagrum.Core.Utilities;
using Flagrum.Web.Features.EarcMods.Data;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Persistence.Entities.ModManager;
using Flagrum.Web.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Flagrum.Console.Ps4.Mogfest;

public class MogfestMaterialReplacer
{
    private readonly Dictionary<string, FmodFragment> _shaders = new();
    private ConcurrentDictionary<string, bool> _shadersToIgnore;
    private readonly ConcurrentDictionary<string, ArchiveFile> _shaderMap = new();
    private EarcModEarc _rootEarc;

    public void Revert()
    {
        // Get the mogfest mod
        using var context = new FlagrumDbContext(new SettingsService());
        var mod = context.EarcMods
            .Include(m => m.Earcs)
            .ThenInclude(e => e.Files)
            .First(m => m.Id == 284);
        
        // Clear any existing shaders from the root earc
        _rootEarc = mod.Earcs.First(e => e.EarcRelativePath == @"level\dlc_ex\mog\area_ravettrice_mog.earc");
        foreach (var shader in _rootEarc.Files.Where(f => f.Uri.EndsWith(".sb")))
        {
            context.Remove(shader);
        }

        context.SaveChanges();
        
        // Process each material
        foreach (var path in Directory.GetFiles(@"C:\Modding\Chocomog\Staging\Fragments", "*.backup"))
        {
            var fragmentPath = path.Replace(".backup", ".ffg");
            File.Copy(path, fragmentPath, true);
        }
    }
    
    public void Run()
    {
        // Map out the default shaders so they aren't packed in
        using var pcContext = new FlagrumDbContext(new SettingsService());
        _shadersToIgnore = new ConcurrentDictionary<string, bool>(pcContext.AssetUris
            .Where(a => a.ArchiveLocation.Path == @"shader\shadergen\autoexternal.earc")
            .ToDictionary(a => a.Uri, a => true));
        
        // Load the material overrides
        var json = File.ReadAllText($@"C:\Modding\Chocomog\Staging\material_overrides.json");
        var map = JsonConvert.DeserializeObject<Dictionary<string, string>>(json)!;
        
        // Get the mogfest mod
        using var context = new FlagrumDbContext(new SettingsService());
        var mod = context.EarcMods
            .Include(m => m.Earcs)
            .ThenInclude(e => e.Files)
            .First(m => m.Id == 284);
        
        // Clear any existing shaders from the root earc
        _rootEarc = mod.Earcs.First(e => e.EarcRelativePath == @"level\dlc_ex\mog\area_ravettrice_mog.earc");
        foreach (var shader in _rootEarc.Files.Where(f => f.Uri.EndsWith(".sb")))
        {
            context.Remove(shader);
        }

        context.SaveChanges();

        // Process each material
        foreach (var (ps4, sample) in map)
        {
            var hash = Cryptography.HashFileUri64(ps4);
            var fragmentPath = $@"C:\Modding\Chocomog\Staging\Fragments\{hash}.ffg";
            var backupPath = $@"C:\Modding\Chocomog\Staging\Fragments\{hash}.backup";

            if (!File.Exists(backupPath))
            {
                File.Copy(fragmentPath, backupPath);
            }

            var newFragment = BuildMaterial(ps4, sample);
            newFragment.Write(fragmentPath);
        }
        
        // Write shader fragments
        foreach (var (uri, fragment) in _shaders)
        {
            var hash = Cryptography.HashFileUri64(uri);
            var fragmentPath = $@"C:\Modding\Chocomog\Staging\Fragments\{hash}.ffg";

            if (!File.Exists(fragmentPath))
            {
                fragment.Write(fragmentPath);
            }
            
            _rootEarc.Files.Add(new EarcModFile
            {
                Uri = uri,
                ReplacementFilePath = fragmentPath,
                Type = EarcFileChangeType.Add,
                Flags = ArchiveFileFlag.Autoload
            });
        }

        context.SaveChanges();
    }

    private FmodFragment BuildMaterial(string ps4Uri, string pcUri)
    {
        var ps4MaterialData = MogfestUtilities.GetFileByUri(ps4Uri);
        var originalMaterial = new MaterialReader(ps4MaterialData).Read();

        using var pcContext = new FlagrumDbContext(new SettingsService());
        var file = pcContext.GetArchiveFileByUri(pcUri);
        var material = new MaterialReader(file.GetReadableData()).Read();

        foreach (var shader in material.Header.Dependencies
                     .Where(d => d.Path.EndsWith(".sb")))
        {
            if (_shadersToIgnore.ContainsKey(shader.Path))
            {
                continue;
            }

            if (!_shaderMap.TryGetValue(shader.Path, out var shaderFile))
            {
                shaderFile = pcContext.GetArchiveFileByUri(shader.Path);
                _shaderMap[shader.Path] = shaderFile;
            }

            var shaderFragment = new FmodFragment
            {
                OriginalSize = shaderFile.Size,
                ProcessedSize = shaderFile.ProcessedSize,
                Flags = shaderFile.Flags | ArchiveFileFlag.Autoload,
                Key = shaderFile.Key,
                RelativePath = shaderFile.RelativePath,
                Data = shaderFile.GetRawData()
            };

            _shaders[shader.Path] = shaderFragment;
        }

        material.HighTexturePackAsset = originalMaterial.HighTexturePackAsset;
        material.Name = originalMaterial.Name;
        material.NameHash = originalMaterial.NameHash;
        material.Uri = ps4Uri;

        foreach (var input in originalMaterial.InterfaceInputs)
        {
            var match = material.InterfaceInputs.FirstOrDefault(i => i.ShaderGenName == input.ShaderGenName);
            if (match != null)
            {
                match.Values = input.Values;
            }
        }

        // Update all the texture slots with the PS4 textures
        foreach (var texture in material.Textures.Where(t => !t.Path.EndsWith(".sb")))
        {
            var match = originalMaterial.Textures.FirstOrDefault(t => t.ShaderGenName.Replace("_", "").ToLower() == texture.ShaderGenName.Replace("_", "").ToLower());
            if (match == null)
            {
                if (!texture.Path.StartsWith("data://shader"))
                {
                    texture.Path = texture.ShaderGenName.Contains("MRS") ? "data://shader/defaulttextures/white.tif" : "data://shader/defaulttextures/black.tif";
                    texture.PathHash = Cryptography.Hash32("data://shader/defaulttextures/black.tif");
                    texture.ResourceFileHash =
                        Cryptography.HashFileUri64("data://shader/defaulttextures/black.tif");
                }
            }
            else
            {
                texture.Path = match.Path;
                texture.PathHash = match.PathHash;
                texture.ResourceFileHash = match.ResourceFileHash;
            }
        }

        material.RegenerateDependencyTable();

        var materialResult = new MaterialWriter(material).Write();
        file.SetRawData(materialResult);
        var processedMaterial = file.GetDataForExport();
        
        return new FmodFragment
        {
            OriginalSize = (uint)materialResult.Length,
            ProcessedSize = (uint)processedMaterial.Length,
            Flags = file.Flags,
            Key = file.Key,
            RelativePath = ps4Uri.Replace("data://", "") + ".gfxbin",
            Data = processedMaterial
        };
    }
}