using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using SequenceEditor.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace SequenceEditor.Pages
{
    public partial class Index
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

        private void Diagram_MouseUp(Blazor.Diagrams.Core.Models.Base.Model sender, Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
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
}
