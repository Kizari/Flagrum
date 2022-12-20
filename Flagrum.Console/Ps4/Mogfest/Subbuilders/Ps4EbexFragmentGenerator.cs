using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;
using Flagrum.Web.Features.EarcMods.Data;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Services;
using Newtonsoft.Json;

namespace Flagrum.Console.Ps4.Porting;

public class Ps4EbexFragmentGenerator
{
    private readonly ConcurrentDictionary<string, bool> _assets = new();
    private readonly ConcurrentDictionary<string, bool> _dependencies = new();
    private readonly SettingsService _pcSettings = new();

    public void Run()
    {
        // Clear existing fragments
        foreach (var file in Directory.EnumerateFiles(@"C:\Modding\Chocomog\Staging\Fragments"))
        {
            File.Delete(file);
        }

        using var context = Ps4Utilities.NewContext();
        using var pcContext = new FlagrumDbContext(_pcSettings);
        CreateEarcRecursively(
            context.FestivalDependencies.First(d => d.Uri == "data://level/dlc_ex/mog/area_ravettrice_mog.ebex"));

        var assets = _assets
            .Where(kvp => !pcContext.AssetUris
                .Any(a => a.Uri == kvp.Key))
            .Select(kvp => kvp.Key)
            .ToList();

        var json = JsonConvert.SerializeObject(assets, Formatting.Indented);
        File.WriteAllText(@$"{Ps4PorterConfiguration.StagingDirectory}\assets.json", json);
    }

    private void CreateEarcRecursively(FestivalDependency ebex)
    {
        lock (ebex)
        {
            if (_dependencies.ContainsKey(ebex.Uri))
            {
                return;
            }

            _dependencies.TryAdd(ebex.Uri, true);
        }

        using var context = Ps4Utilities.NewContext();
        var pcContext = new FlagrumDbContext(_pcSettings);

        var isDuplicate = pcContext.AssetUris.Any(a => a.Uri == ebex.Uri);
        if (isDuplicate)
        {
            return;
        }

        var exml = Ps4Utilities.GetArchiveFileByUri(context, ebex.Uri);
        var fragment = new FmodFragment
        {
            OriginalSize = exml.Size,
            ProcessedSize = exml.ProcessedSize,
            Flags = ArchiveFileFlag.Autoload,
            Key = exml.Key,
            RelativePath = exml.RelativePath,
            Data = exml.GetRawData()
        };

        var hash = Cryptography.HashFileUri64(ebex.Uri);
        fragment.Write($@"C:\Modding\Chocomog\Staging\Fragments\{hash}.ffg");

        var children = context.FestivalDependencyFestivalDependency
            .Where(d => d.ParentId == ebex.Id)
            .Select(d => d.Child)
            .ToList();

        Parallel.ForEach(children, CreateEarcRecursively);

        // The rest of this just generates the asset list for the other builders
        var subdependencies = context.FestivalDependencyFestivalSubdependency
            .Where(d => d.DependencyId == ebex.Id)
            .Select(d => d.Subdependency)
            .ToList();

        foreach (var subdependency in subdependencies)
        {
            _assets.TryAdd(subdependency.Uri.ToLower(), true);

            var modelDependencies = context.FestivalSubdependencyFestivalModelDependency
                .Where(s => s.SubdependencyId == subdependency.Id)
                .Select(s => s.ModelDependency)
                .ToList();

            foreach (var modelDependency in modelDependencies)
            {
                _assets.TryAdd(modelDependency.Uri.ToLower(), true);

                var materialDependencies = context.FestivalModelDependencyFestivalMaterialDependency
                    .Where(m => m.ModelDependencyId == modelDependency.Id)
                    .Select(m => m.MaterialDependency)
                    .ToList();

                foreach (var materialDependency in materialDependencies)
                {
                    if (materialDependency.Uri.EndsWith(".htpk"))
                    {
                        _assets.TryAdd(materialDependency.Uri.ToLower(), true);
                    }
                    else
                    {
                        _assets.TryAdd(materialDependency.Uri.ToLower(), true);
                    }
                }
            }
        }
    }
}