using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;
using Flagrum.Web.Features.EarcMods.Data;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Services;

namespace Flagrum.Console.Ps4.Porting;

public class Ps4EbexFragmentGenerator
{
    private readonly SettingsService _pcSettings = new();
    private readonly ConcurrentDictionary<string, bool> _dependencies = new();

    public void Run()
    {
        using var context = Ps4Utilities.NewContext();
        CreateEarcRecursively(context.FestivalDependencies.First(d => d.Uri == "data://level/dlc_ex/mog/area_ravettrice_mog.ebex"));
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
    }
}