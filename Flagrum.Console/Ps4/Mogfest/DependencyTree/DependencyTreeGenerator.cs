using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Console.Ps4.Mogfest.Utilities;
using Flagrum.Core.AI;
using Flagrum.Core.Ebex.Xmb2;
using Flagrum.Web.Persistence.Entities;

namespace Flagrum.Console.Ps4.Mogfest.DependencyTree;

public class DependencyTreeGenerator
{
    private readonly Dictionary<uint, string> _archetypeMap = MogfestUtilities.GetArchetypeMap();
    private readonly Dictionary<string, FestivalDependency> _dependencies = new();
    private readonly List<FestivalDependencyFestivalDependency> _dependencyLinks = new();
    private readonly object _lock = new();

    public void Run()
    {
        using var context = Ps4Utilities.NewContext();
        context.ClearTable(nameof(context.FestivalDependencies));
        context.ClearTable(nameof(context.FestivalDependencyFestivalDependency));
        const string uri = "data://level/dlc_ex/mog/area_ravettrice_mog.ebex";

        var data = MogfestUtilities.GetFileByUri(uri);
        _ = TraverseEbexTree(uri, data, null);

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

        using var stream = new MemoryStream(xmb2);
        var root = Xmb2Document.GetRootElement(stream);
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
                                                        || e.Name.Contains("GraphData",
                                                            StringComparison.OrdinalIgnoreCase)
                                                        || e.Name.Contains("Command",
                                                            StringComparison.OrdinalIgnoreCase)))
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

            if (element.GetAttributeByName("type").GetTextValue() == "Black.AI.Ambient.AmbientSpawnPointEntity")
            {
                var archeType = subElements.FirstOrDefault(s => s.Name == "archeType_");
                if (archeType != null)
                {
                    var archeTypeId = archeType.GetAttributeByName("fixid").ToUInt();
                    if (archeTypeId > 0)
                    {
                        if (_archetypeMap.TryGetValue(archeTypeId, out var archetypeUri))
                        {
                            AddUri(archetypeUri, uri, current);
                        }
                    }
                }
            }
            
            if (element.GetAttributeByName("type").GetTextValue() == "Black.AI.Ambient.AmbientPopulationEntryEntity")
            {
                var archeType = subElements.FirstOrDefault(s => s.Name == "id_");
                if (archeType != null)
                {
                    var archeTypeId = archeType.GetAttributeByName("fixid").ToUInt();
                    if (archeTypeId > 0)
                    {
                        if (_archetypeMap.TryGetValue(archeTypeId, out var archetypeUri))
                        {
                            AddUri(archetypeUri, uri, current);
                        }
                    }
                }
            }
            
            var aiiaXmls = subElements
                .Where(e => e.Name == "interactionFile_")
                .Select(p => p.GetTextValue());
            foreach (var aiiaXml in aiiaXmls)
            {
                AddAiiaDependencies(aiiaXml, uri, current);
            }
        });

        GC.Collect();
        return current;
    }

    private void AddUriFromDependency(Xmb2Element element, string pathElementName, string uri,
        FestivalDependency current)
    {
        var path = element.GetElementByName(pathElementName)?.GetTextValue();
        AddUri(path, uri, current);
    }

    private void AddUri(string path, string uri, FestivalDependency current)
    {
        if (path != null && (path.EndsWith(".ebex") || path.EndsWith(".prefab")))
        {
            var combinedUriString = RelativeUriToUri(path, uri);
            var innerXmb2Path = MogfestUtilities.UriToFilePath(combinedUriString);
            if (File.Exists(innerXmb2Path))
            {
                _ = TraverseEbexTree(combinedUriString, File.ReadAllBytes(innerXmb2Path), current);
            }
        }
    }

    private string RelativeUriToUri(string relativeUri, string baseUri)
    {
        relativeUri = relativeUri.Replace('\\', '/').ToLower();

        string combinedUriString;
        if (relativeUri.StartsWith('.'))
        {
            var uri = new Uri(baseUri.Replace("data://", "data://data/"));
            var combinedUri = new Uri(uri, relativeUri);
            combinedUriString = combinedUri.ToString().Replace("data://data/", "data://");
        }
        else
        {
            combinedUriString = $"data://{relativeUri}";
        }

        return combinedUriString;
    }

    private void AddAiiaDependencies(string aiiaRefUri, string baseUri, FestivalDependency current)
    {
        aiiaRefUri = RelativeUriToUri(aiiaRefUri, baseUri);
        var data = MogfestUtilities.GetFileByUri(aiiaRefUri);
        if (data.Length == 0)
        {
            return;
        }
        
        foreach (var uri in AiiaRef.GetDependencies(data).Where(u => u.EndsWith(".ebex")))
        {
            var innerXmb2Path = MogfestUtilities.UriToFilePath(uri);
            if (File.Exists(innerXmb2Path))
            {
                _ = TraverseEbexTree(uri, File.ReadAllBytes(innerXmb2Path), current);
            }
        }
    }
}