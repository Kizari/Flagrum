using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Black.Entity;
using Flagrum.Core.Ebex.Xmb2;
using Flagrum.Web.Persistence.Entities;
using SQEX.Ebony.Framework.Entity;

namespace Flagrum.Console.Ps4.Porting;

public class Ps4SubdependencyTreeBuilder
{
    private readonly object _lock = new();
    private readonly Dictionary<string, FestivalSubdependency> _subdependencies = new();
    private readonly List<FestivalDependencyFestivalSubdependency> _links = new();

    public void Run()
    {
        using var outerContext = Ps4Utilities.NewContext();
        outerContext.ClearTable(nameof(outerContext.FestivalSubdependencies));
        outerContext.ClearTable(nameof(outerContext.FestivalDependencyFestivalSubdependency));
        
        var dependencies = outerContext.FestivalDependencies.ToList();
        Parallel.ForEach(dependencies, dependency =>
        {
            using var context = Ps4Utilities.NewContext();
            var xmb2 = Ps4Utilities.GetFileByUri(context, dependency.Uri);
            var root = Xmb2Document.GetRootElement(xmb2);
            var objects = root.GetElementByName("objects");
            var elements = objects.GetElements();
    
            foreach (var element in elements)
            {
                var typeAttribute = element.GetAttributeByName("type").GetTextValue();
                var type = Assembly.GetAssembly(typeof(EntityPackageReference))!.GetTypes()
                    .FirstOrDefault(t => t.FullName?.Contains(typeAttribute) == true);
                if (type == null || !type.IsAssignableTo(typeof(EntityPackageReference)))
                {
                    var subelements = element.GetElements();
                    var aiiaXmls = subelements
                        .Where(e => e.Name == "interactionFile_")
                        .Select(p => p.GetTextValue());
    
                    var paths = subelements
                        .Where(e => e.Name.Contains("path", StringComparison.OrdinalIgnoreCase)
                                    || e.Name is "LmAnimMdlData" or "sourceFileName_")
                        .Select(p => p.GetTextValue())
                        .ToList();
    
                    foreach (var aiiaXml in aiiaXmls)
                    {
                        paths.Add(aiiaXml);
                        paths.Add(aiiaXml.Replace(".aiia.xml", ".aiia"));
                        paths.Add(aiiaXml.Replace(".aiia.xml", ".aiia.dbg"));
                    }
                    
                    foreach (var path in paths)
                    {
                        AddUriByRelativePath(path, dependency);
                    }
    
                    var lists = subelements
                        .Where(e => e.Name.Contains("List", StringComparison.OrdinalIgnoreCase) ||
                                    e.Name == "ExtraSoundFileNames");
    
                    foreach (var list in lists)
                    {
                        foreach (var item in list.GetElements())
                        {
                            AddUriFromSubdependency(item, "Value", dependency);
                        }
                    }
    
                    var differences = subelements.FirstOrDefault(e => e.Name == "differences");
                    if (differences != null)
                    {
                        foreach (var difference in differences.GetElements())
                        {
                            AddUriFromSubdependency(difference, "sourcePath_", dependency);
                        }
                    }
    
                    if (type != null && type.IsAssignableTo(typeof(StaticModelEntity)))
                    {
                        AddUriFromSubdependency(element, "MeshCollision_", dependency);
                    }

                    var timeLine = subelements.FirstOrDefault(e => e.Name == "timeLine_");
                    if (timeLine != null)
                    {
                        AddUriFromSubdependency(timeLine, "sourcePath_", dependency);
                        var childTrackList = timeLine.GetElementByName("childTrackList_");
                        if (childTrackList != null)
                        {
                            foreach (var group in childTrackList.GetElements())
                            {
                                AddUriFromSubdependency(group, "sourcePath_", dependency);
                                var innerChildTrackList = group.GetElementByName("childTrackList_");
                                if (innerChildTrackList != null)
                                {
                                    foreach (var track in innerChildTrackList.GetElements())
                                    {
                                        AddUriFromSubdependency(track, "sourcePath_", dependency);
                                        var trackItemList = track.GetElementByName("trackItemList_");
                                        if (trackItemList != null)
                                        {
                                            foreach (var trackItem in trackItemList.GetElements())
                                            {
                                                AddUriFromSubdependency(trackItem, "sourcePath_", dependency);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    var subelements = element.GetElements();
                    var lists = subelements
                        .Where(e => e.Name.Contains("List", StringComparison.OrdinalIgnoreCase));
    
                    foreach (var list in lists)
                    {
                        foreach (var item in list.GetElements())
                        {
                            AddUriFromSubdependency(item, "Value", dependency);
                        }
                    }
                }
            }
        });
    
        outerContext.FestivalDependencyFestivalSubdependency.AddRange(_links);
        outerContext.SaveChanges();
    }
    
    private void AddUriFromSubdependency(Xmb2Element element, string pathElementName, FestivalDependency dependency)
    {
        var path = element.GetElementByName(pathElementName)?.GetTextValue();
        AddUriByRelativePath(path, dependency);
    }

    private void AddUriByRelativePath(string path, FestivalDependency dependency)
    {
        if (path != null && !path.EndsWith(".ebex") && !path.EndsWith(".prefab") && path.Contains('.'))
        {
            path = path.Replace('\\', '/').ToLower();
            if (path.StartsWith("data://"))
            {
                path = path.Replace("data://", "");
            }
                
            string combinedUriString;
            if (path.StartsWith('.'))
            {
                var uriUri = new Uri(dependency.Uri.Replace("data://", "data://data/"));
                var combinedUri = new Uri(uriUri, path);
                combinedUriString = combinedUri.ToString().Replace("data://data/", "data://");
            }
            else
            {
                combinedUriString = $"data://{path}";
            }
            
            lock (_lock)
            {
                _subdependencies.TryGetValue(combinedUriString, out var subdependency);
                if (subdependency == null)
                {
                    subdependency = new FestivalSubdependency {Uri = combinedUriString};
                    _subdependencies.Add(combinedUriString, subdependency);
                }

                if (!_links.Any(l => l.Dependency == dependency && l.Subdependency == subdependency))
                {
                    _links.Add(new FestivalDependencyFestivalSubdependency
                    {
                        Dependency = dependency,
                        Subdependency = subdependency
                    });
                }
            }
        }
    }
}