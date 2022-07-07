using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Core.Ebex.Xmb2;
using Flagrum.Web.Persistence.Entities;

namespace Flagrum.Console.Ps4.Porting;

public class Ps4DependencyTreeBuilder
{
    private readonly Dictionary<string, FestivalDependency> _dependencies = new();
    private readonly List<FestivalDependencyFestivalDependency> _dependencyLinks = new();
    private readonly object _lock = new();
    
    public void Run()
    {
        using var context = Ps4Utilities.NewContext();

        context.ClearTable(nameof(context.FestivalDependencies));
        context.ClearTable(nameof(context.FestivalDependencyFestivalDependency));
        const string uri = "data://level/dlc_ex/mog/area_ravettrice_mog.ebex";
        var data = Ps4Utilities.GetFileByUri(context, uri);
        
        var root = TraverseEbexTree(uri, data, null);

        var edgeCases = context.Ps4AssetUris.Where(a =>
                a.Uri.StartsWith("data://character/") && a.Uri.Contains("/entry/") && a.Uri.EndsWith("_mog.ebex"))
            .Select(a => a.Uri)
            .ToList();
        
        edgeCases.Add("data://character/um/um20/entry/um20_001_hair00.ebex");
        
        foreach (var ebexUri in edgeCases.Where(ebexUri => !_dependencyLinks.Any(l => l.Child.Uri == ebexUri)))
        {
            _dependencyLinks.Add(new FestivalDependencyFestivalDependency
            {
                Parent = root,
                Child = new FestivalDependency {Uri = ebexUri}
            });
        }
        
        context.FestivalDependencyFestivalDependency.AddRange(_dependencyLinks);
        context.SaveChanges();
        context.ChangeTracker.Clear();
    }
    
    private FestivalDependency TraverseEbexTree(string uri, byte[] xmb2, FestivalDependency parent)
    {
        uri = uri.ToLower();
        FestivalDependency current;

        lock (_lock)
        {
            var alreadyTraversed = _dependencies.ContainsKey(uri);

            if (alreadyTraversed)
            {
                current = _dependencies[uri];
                if (_dependencyLinks.Any(l => l.Parent == parent && l.Child == current))
                {
                    return null;
                }
                
                if (parent != null)
                {
                    _dependencyLinks.Add(new FestivalDependencyFestivalDependency
                    {
                        Parent = parent,
                        Child = current
                    });
                }

                return null;
            }

            current = new FestivalDependency {Uri = uri};
            _dependencies.TryAdd(uri, current);
            
            if (parent != null)
            {
                _dependencyLinks.Add(new FestivalDependencyFestivalDependency
                {
                    Parent = parent,
                    Child = current
                });
            }
        }
        
        var root = Xmb2Document.GetRootElement(xmb2);
        var objects = root.GetElementByName("objects");
        var elements = objects.GetElements();

        Parallel.ForEach(elements, element =>
        {
            var subElements = element.GetElements();
            
            var differences = subElements.FirstOrDefault(e => e.Name == "differences");
            if (differences != null)
            {
                foreach (var difference in differences.GetElements())
                {
                    AddUriFromDependency(difference, "sourcePath_", uri, current);
                }
            }

            var diffList = subElements.FirstOrDefault(e => e.Name == "prefabDiffResourcePathList_");
            if (diffList != null)
            {
                foreach (var difference in diffList.GetElements())
                {
                    AddUriFromDependency(difference, "Value", uri, current);
                }
            }
            
            // Ignore the rest of the self-declaration
            if (element == elements.First())
            {
                return;
            }

            foreach (var item in subElements.Where(e => e.Name.Contains("path", StringComparison.OrdinalIgnoreCase)
                     || e.Name.Contains("GraphData", StringComparison.OrdinalIgnoreCase)
                     || e.Name.Contains("Command", StringComparison.OrdinalIgnoreCase)))
            {
                AddUriFromDependency(element, item.Name, uri, current);
            }
            
            AddUriFromDependency(element, "charaEntry_", uri, current);
            AddUriFromDependency(element, "CharaEntry_", uri, current);
            AddUriFromDependency(element, "swfEntryPackagePath_", uri, current);

            foreach (var list in subElements.Where(e => e.Name.Contains("List", StringComparison.OrdinalIgnoreCase)))
            {
                foreach (var item in list.GetElements())
                {
                    AddUriFromDependency(item, "Value", uri, current);
                }
            }
        });

        GC.Collect();
        return current;
    }
    
    private void AddUriFromDependency(Xmb2Element element, string pathElementName, string uri, FestivalDependency current)
    {
        var path = element.GetElementByName(pathElementName)?.GetTextValue();
        if (path != null && (path.EndsWith(".ebex") || path.EndsWith(".prefab")))
        {
            string combinedUriString;
            if (path.StartsWith('.'))
            {
                var uriUri = new Uri(uri.Replace("data://", "data://data/"));
                var combinedUri = new Uri(uriUri, path);
                combinedUriString = combinedUri.ToString().Replace("data://data/", "data://");
            }
            else
            {
                combinedUriString = $"data://{path}";
            }

            byte[] innerXmb2;
            using (var context = Ps4Utilities.NewContext())
            {
                innerXmb2 = Ps4Utilities.GetFileByUri(context, combinedUriString);
            }

            if (innerXmb2.Length > 0)
            {
                _ = TraverseEbexTree(combinedUriString, innerXmb2, current);
            }
        }
    }
}