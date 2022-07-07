using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Flagrum.Core.Gfxbin.Data;
using Flagrum.Core.Gfxbin.Gmdl.Components;
using Flagrum.Core.Gfxbin.Gmtl;
using Flagrum.Core.Gfxbin.Serialization;
using Flagrum.Core.Vfx;
using Flagrum.Web.Persistence.Entities;

namespace Flagrum.Console.Ps4.Porting;

public class Ps4ModelDependencyTreeBuilder
{
    private readonly List<FestivalSubdependencyFestivalModelDependency> _links = new();
    private readonly object _lock = new();
    private readonly Dictionary<string, FestivalModelDependency> _modelDependencies = new();
    private readonly Dictionary<string, FestivalSubdependency> _vfxDependencies = new();
    private readonly List<FestivalDependencyFestivalSubdependency> _vfxLinks = new();

    public void Run()
    {
        using var context = Ps4Utilities.NewContext();
        context.ClearTable(nameof(context.FestivalModelDependencies));
        context.ClearTable(nameof(context.FestivalSubdependencyFestivalModelDependency));

        foreach (var dependency in context.FestivalSubdependencies.Where(d => d.Uri.EndsWith(".vlink")))
        {
            var data = Ps4Utilities.GetFileByUri(context, dependency.Uri);
            if (data.Length == 0)
            {
                continue;
            }

            var uri = Vlink.GetVfxUriFromData(data);
            AddSubdependency(uri, dependency);
        }

        context.FestivalDependencyFestivalSubdependency.AddRange(_vfxLinks);
        context.SaveChanges();
        _vfxLinks.Clear();

        Parallel.ForEach(context.FestivalSubdependencies.Where(s => s.Uri.EndsWith(".vfx")), vfx =>
        {
            using var innerContext = Ps4Utilities.NewContext();
            var data = Ps4Utilities.GetFileByUri(innerContext, vfx.Uri);
            if (data.Length == 0)
            {
                return;
            }

            var matches = Regex.Matches(Encoding.UTF8.GetString(data), @"data:\/\/.+?\..+?" + (char)0x00);
            foreach (Match match in matches)
            {
                var uri = match.Value[..^1];
                AddSubdependency(uri, vfx);
            }
        });

        context.FestivalDependencyFestivalSubdependency.AddRange(_vfxLinks);
        context.SaveChanges();

        Parallel.ForEach(context.FestivalSubdependencies.Where(d => d.Uri.EndsWith(".gmdl")), dependency =>
        {
            using var innerContext = Ps4Utilities.NewContext();
            var data = Ps4Utilities.GetFileByUri(innerContext, dependency.Uri);

            if (data.Length == 0)
            {
                return;
            }

            var header = new GfxbinHeader();
            header.Read(new BinaryReader(data));
            foreach (var subdependency in header.Dependencies.Where(subdependency =>
                         subdependency.PathHash != "asset_uri" && subdependency.PathHash != "ref"))
            {
                AddModelDependency(subdependency.Path, dependency);
            }
            // var gpubin = Ps4Utilities.GetFileByUri(innerContext, dependency.Uri.Replace(".gmdl", ".gpubin"));
            // var model = new ModelReader(data, gpubin).Read();
            // foreach (var subdependency in model.Header.Dependencies)
            // {
            //     if (subdependency.Path.EndsWith(".gmtl"))
            //     {
            //         var hash = ulong.Parse(subdependency.PathHash);
            //         var mesh = model.MeshObjects
            //             .Select(mo => mo.Meshes
            //                 .FirstOrDefault(m => m.DefaultMaterialHash == hash))
            //             .FirstOrDefault();
            //         
            //         AddDependency(subdependency.Path, dependency, mesh?.VertexLayoutType ?? VertexLayoutType.NULL);
            //     }
            //     else if (subdependency.PathHash != "asset_uri" && subdependency.PathHash != "ref")
            //     {
            //         AddDependency(subdependency.Path, dependency);
            //     }
            // }
        });

        context.FestivalSubdependencyFestivalModelDependency.AddRange(_links);
        context.SaveChanges();
        _links.Clear();

        Parallel.ForEach(context.FestivalSubdependencies.Where(d => d.Uri.EndsWith(".gmtl")), dependency =>
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
                AddModelDependency(subdependency.Path, dependency);
            }

            if (!string.IsNullOrEmpty(material.HighTexturePackAsset))
            {
                AddModelDependency(material.HighTexturePackAsset, dependency);
            }
        });

        context.FestivalSubdependencyFestivalModelDependency.AddRange(_links);
        context.SaveChanges();
    }

    private void AddSubdependency(string uri, FestivalSubdependency subdependency)
    {
        using var context = Ps4Utilities.NewContext();
        uri = uri.Replace('\\', '/').ToLower();

        lock (_lock)
        {
            _vfxDependencies.TryGetValue(uri, out var vfxDependency);
            if (vfxDependency == null)
            {
                vfxDependency = new FestivalSubdependency {Uri = uri};
                _vfxDependencies.Add(uri, vfxDependency);
            }

            foreach (var parentId in context.FestivalSubdependencies
                         .Where(s => s.Id == subdependency.Id)
                         .SelectMany(s => s.Parents.Select(p => p.DependencyId)))
            {
                if (!_vfxLinks.Any(l => l.DependencyId == parentId && l.Subdependency == vfxDependency))
                {
                    _vfxLinks.Add(new FestivalDependencyFestivalSubdependency
                    {
                        DependencyId = parentId,
                        Subdependency = vfxDependency
                    });
                }
            }
        }
    }

    private void AddModelDependency(string uri, FestivalSubdependency subdependency,
        VertexLayoutType vertexLayoutType = VertexLayoutType.NULL)
    {
        uri = uri.Replace('\\', '/').ToLower();

        lock (_lock)
        {
            _modelDependencies.TryGetValue(uri, out var modelDependency);
            if (modelDependency == null)
            {
                modelDependency = new FestivalModelDependency {Uri = uri, VertexLayoutType = vertexLayoutType};
                _modelDependencies.Add(uri, modelDependency);
            }

            if (!_links.Any(l => l.Subdependency == subdependency && l.ModelDependency == modelDependency))
            {
                _links.Add(new FestivalSubdependencyFestivalModelDependency
                {
                    Subdependency = subdependency,
                    ModelDependency = modelDependency
                });
            }
        }
    }
}