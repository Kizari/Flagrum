using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using Flagrum.Core.Utilities;
using Flagrum.Web.Components.Graph;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using SQEX.Ebony.Framework.Entity;

namespace Flagrum.Web.Features.SequenceEditor;

public partial class SequenceEditor
{
    private string _autosavePath;

    [Inject] private IJSRuntime JSRuntime { get; set; }
    [Inject] private SettingsService Settings { get; set; }
    [Inject] private FlagrumDbContext Context { get; set; }

    [Parameter] public string UriBase64 { get; set; }

    private Diagram Diagram { get; set; }

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

            var counter = 1;
            foreach (var entity in package.entities_)
            {
                var node = new StandardNode(entity.GetType(), new Point(300 * counter, 50));
                Diagram.Nodes.Add(node);
                counter++;
            }
        }
        else
        {
            LoadData();
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
        var persistence = new DiagramPersistence();

        string json = null;
        if (File.Exists(_autosavePath))
        {
            json = await File.ReadAllTextAsync(_autosavePath);
        }

        if (json == null)
        {
            persistence.New(Diagram);
        }
        else
        {
            try
            {
                var data = JsonConvert.DeserializeObject<DiagramData>(json);
                await Task.Delay(50);
                persistence.Load(Diagram, data);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                persistence.New(Diagram);
            }
        }
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