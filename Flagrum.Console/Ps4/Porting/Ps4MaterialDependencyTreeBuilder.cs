using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Core.Gfxbin.Gmtl;
using Flagrum.Web.Persistence.Entities;

namespace Flagrum.Console.Ps4.Porting;

public class Ps4MaterialDependencyTreeBuilder
{
    private readonly List<FestivalModelDependencyFestivalMaterialDependency> _links = new();
    private readonly object _lock = new();
    private readonly Dictionary<string, FestivalMaterialDependency> _materialDependencies = new();

    public void Run()
    {
        using var context = Ps4Utilities.NewContext();
        context.ClearTable(nameof(context.FestivalMaterialDependencies));
        context.ClearTable(nameof(context.FestivalModelDependencyFestivalMaterialDependency));

        Parallel.ForEach(context.FestivalModelDependencies.Where(d => d.Uri.EndsWith(".gmtl")), dependency =>
        {
            using var innerContext = Ps4Utilities.NewContext();
            var data = Ps4Utilities.GetFileByUri(innerContext, dependency.Uri);

            if (data.Length == 0)
            {
                System.Console.WriteLine($"[E] Data not found for {dependency.Uri}!");
                return;
            }

            var material = new MaterialReader(data).Read();
            foreach (var subdependency in material.Header.Dependencies.Where(subdependency =>
                         subdependency.PathHash is not "asset_uri" and not "ref" &&
                         !subdependency.Path.EndsWith(".sb")))
            {
                AddDependency(subdependency.Path, dependency);
            }

            if (!string.IsNullOrEmpty(material.HighTexturePackAsset))
            {
                AddDependency(material.HighTexturePackAsset, dependency);
            }
        });

        context.FestivalModelDependencyFestivalMaterialDependency.AddRange(_links);
        context.SaveChanges();
    }

    private void AddDependency(string uri, FestivalModelDependency modelDependency)
    {
        uri = uri.Replace('\\', '/').ToLower();

        lock (_lock)
        {
            _materialDependencies.TryGetValue(uri, out var materialDependency);
            if (materialDependency == null)
            {
                materialDependency = new FestivalMaterialDependency {Uri = uri};
                _materialDependencies.Add(uri, materialDependency);
            }

            if (!_links.Any(l => l.ModelDependency == modelDependency && l.MaterialDependency == materialDependency))
            {
                _links.Add(new FestivalModelDependencyFestivalMaterialDependency
                {
                    ModelDependency = modelDependency,
                    MaterialDependency = materialDependency
                });
            }
        }
    }
}