using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using Blazored.LocalStorage;
using Flagrum.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace Flagrum.Web.Features.SequenceEditor;

public partial class SequenceEditor
{
    [Inject] private ISyncLocalStorageService LocalStorage { get; set; }
    [Inject] private IJSRuntime JSRuntime { get; set; }

    private Diagram Diagram { get; set; }

    private Timer SaveTimer { get; set; }

    protected override void OnInitialized()
    {
        InitializeSaveTimer();
        Diagram = new Diagram(CreateDiagramOptions());
        Diagram.RegisterModelComponent<StandardNode, StandardNodeRenderer>();
        Diagram.RegisterModelComponent<StandardGroup, StandardGroupRenderer>();
        Diagram.MouseUp += Diagram_MouseUp;
        LoadData();
    }

    private void Diagram_MouseUp(Model sender, MouseEventArgs args)
    {
        if (sender is StandardNode node && node.Group == null)
        {
            var group = Diagram.Groups.FirstOrDefault(g => g.GetBounds().Overlap(node.GetBounds()));
            if (group != null)
            {
                group.AddChild(node);
            }
        }
    }

    private async void LoadData()
    {
        var persistence = new DiagramPersistence();
        var json = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "SequenceEditorDiagram");
        if (json != null)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<DiagramData>(json);
                persistence.Load(Diagram, data);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                persistence.New(Diagram);
            }
        }
        else
        {
            persistence.New(Diagram);
        }
    }

    private void InitializeSaveTimer()
    {
        SaveTimer = new Timer(10000);
        SaveTimer.Elapsed += async (sender, args) =>
        {
            Console.WriteLine("Saved!");
            var diagramData = DiagramData.FromDiagram(Diagram);
            var json = JsonConvert.SerializeObject(diagramData);
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", "SequenceEditorDiagram", json);
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