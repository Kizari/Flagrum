using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using Flagrum.Core.Utilities;
using Flagrum.Web.Components.Graph;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Services;
using GraphShape;
using GraphShape.Algorithms.Layout;
using GraphShape.Algorithms.OverlapRemoval;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using QuikGraph;
using SQEX.Ebony.Framework.Entity;
using SQEX.Ebony.Framework.Node;
using Point = Blazor.Diagrams.Core.Geometry.Point;
using Size = GraphShape.Size;

namespace Flagrum.Web.Features.SequenceEditor;

public partial class SequenceEditor
{
    private string _autosavePath;

    [Inject] private IJSRuntime JSRuntime { get; set; }
    [Inject] private SettingsService Settings { get; set; }
    [Inject] private FlagrumDbContext Context { get; set; }

    [Parameter] public string UriBase64 { get; set; }

    private Diagram Diagram { get; set; }
    private string LayoutAlgorithm { get; set; }

    private Timer SaveTimer { get; set; }

    protected override void OnInitialized()
    {
        _autosavePath = $"{Settings.TempDirectory}\\sequence_autosave.json";
        InitializeSaveTimer();
        Diagram = new Diagram(CreateDiagramOptions());
        Diagram.RegisterModelComponent<StandardNode, StandardNodeRenderer>();
        Diagram.RegisterModelComponent<StandardGroup, StandardGroupRenderer>();
        Diagram.MouseUp += Diagram_MouseUp;

        if (UriBase64 != null)
        {
            var uri = UriBase64.FromBase64();
            var xmb2 = Context.GetFileByUri(uri);
            var loader = new EntityPackageXmlLoader();
            var package = loader.CreateEntityPackage(xmb2);

            // Add valid node objects to the diagram
            foreach (var @object in package.loadedObjects_)
            {
                if (@object is GraphNode node)
                {
                    var standardNode = new StandardNode(node, new Point(350, 50));
                    Diagram.Nodes.Add(standardNode);
                }
            }

            // Add node links to the diagram
            foreach (var node in Diagram.Nodes)
            {
                foreach (StandardPort sourcePort in node.Ports.Where(p => (p as StandardPort)?.Pin is GraphOutputPin))
                {
                    foreach (var targetPort in sourcePort.Pin.connections_
                                 .Select(c => c?.Port)
                                 .Where(p => p != null))
                    {
                        var newLink = new LinkModel(sourcePort!, targetPort);

                        var color = sourcePort?.Color ?? targetPort.Color;
                        if (color == StandardPort.Control)
                        {
                            newLink.TargetMarker = LinkMarker.NewArrow(16, 8);
                            newLink.Width = 4;
                        }

                        newLink.Color = color;
                        Diagram.Links.Add(newLink);
                    }
                }
            }
        }
        else
        {
            LoadData();
        }
    }

    private void RunLayout()
    {
        // convert Z.Blazor.Diagram to QuikGraph
        var graph = new CompoundGraph<NodeModel, Edge<NodeModel>>();
        var nodes = Diagram.Nodes.OfType<NodeModel>().ToList();
        var edges = Diagram.Links.OfType<LinkModel>()
            .Select(lm =>
            {
                var source = nodes.Single(dn => dn.Id == lm.SourceNode.Id);
                var target = nodes.Single(dn => dn.Id == lm?.TargetNode?.Id);
                return new Edge<NodeModel>(source, target);
            })
            .ToList();
        graph.AddVertexRange(nodes);
        graph.AddEdgeRange(edges);

        // run GraphShape algorithm
        var positions = nodes.ToDictionary(nm => nm, dn => new GraphShape.Point(dn.Position.X, dn.Position.Y));
        var sizes = nodes.ToDictionary(nm => nm, dn => new Size(dn.Size?.Width ?? 300, dn.Size?.Height ?? 300));
        var rectangles = nodes.ToDictionary(nm => nm,
            dn => new Rect(dn.Position.X, dn.Position.Y, dn.Size?.Width ?? 300, dn.Size?.Height ?? 300));
        var layoutCtx =
            new LayoutContext<NodeModel, Edge<NodeModel>, CompoundGraph<NodeModel, Edge<NodeModel>>>(graph,
                positions, sizes, LayoutMode.Simple);
        var algoFact =
            new StandardLayoutAlgorithmFactory<NodeModel, Edge<NodeModel>,
                CompoundGraph<NodeModel, Edge<NodeModel>>>();
        var algo = algoFact.CreateAlgorithm(LayoutAlgorithm, layoutCtx, new SugiyamaLayoutParameters
        {
            Direction = LayoutDirection.LeftToRight,
            EdgeRouting = SugiyamaEdgeRouting.Traditional,
            LayerGap = 100,
            MinimizeEdgeLength = false,
            OptimizeWidth = false,
            SliceGap = 150
        });
        algo.Compute();

        // update NodeModel positions
        try
        {
            Diagram.SuspendRefresh = true;
            foreach (var vertPos in algo.VerticesPositions)
            {
                // NOTE;  have to use SetPosition which takes care of updating everything
                vertPos.Key.SetPosition(vertPos.Value.X, vertPos.Value.Y);
            }
        }
        finally
        {
            Diagram.SuspendRefresh = false;
        }
    }

    private void Diagram_MouseUp(Model sender, MouseEventArgs args)
    {
        if (sender is StandardNode node && node.Group == null)
        {
            var group = Diagram.Groups.FirstOrDefault(g => g.GetBounds().Overlap(node.GetBounds()));
            group?.AddChild(node);
        }
    }

    private async void LoadData()
    {
        // var persistence = new DiagramPersistence();
        //
        // string json = null;
        // if (File.Exists(_autosavePath))
        // {
        //     json = await File.ReadAllTextAsync(_autosavePath);
        // }
        //
        // if (json == null)
        // {
        //     persistence.New(Diagram);
        // }
        // else
        // {
        //     try
        //     {
        //         var data = JsonConvert.DeserializeObject<DiagramData>(json);
        //         await Task.Delay(50);
        //         persistence.Load(Diagram, data);
        //         StateHasChanged();
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.Write(ex.Message);
        //         persistence.New(Diagram);
        //     }
        // }
    }

    private void InitializeSaveTimer()
    {
        SaveTimer = new Timer(10000);
        SaveTimer.Elapsed += async (sender, args) =>
        {
            var diagramData = DiagramData.FromDiagram(Diagram);
            var json = JsonConvert.SerializeObject(diagramData);
            await File.WriteAllTextAsync(_autosavePath, json);
        };

        SaveTimer.Start();
    }

    private DiagramOptions CreateDiagramOptions()
    {
        return new DiagramOptions
        {
            Groups = new DiagramGroupOptions
            {
                Enabled = true,
                Factory = GroupFactory
            },
            Links = new DiagramLinkOptions
            {
                Factory = LinkFactory
            },
            Zoom = new DiagramZoomOptions
            {
                Inverse = true
            }
        };
    }

    private LinkModel LinkFactory(Diagram diagram, PortModel sourcePort)
    {
        var link = new LinkModel(sourcePort);

        if (sourcePort is StandardPort port)
        {
            link.Color = port.Color;

            if (port.Color == StandardPort.Control)
            {
                link.TargetMarker = LinkMarker.NewArrow(16, 8);
                link.Width = 4;
            }
        }

        return link;
    }

    private GroupModel GroupFactory(Diagram diagram, IEnumerable<NodeModel> children)
    {
        var group = new GroupModel(children, 25);
        group.AddPort(PortAlignment.Left);
        group.AddPort(PortAlignment.Right);
        return group;
    }
}