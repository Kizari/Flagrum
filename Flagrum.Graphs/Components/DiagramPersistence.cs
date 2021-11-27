using Black.Sequence.Action.EventScript;
using Black.Sequence.Actor;
using Black.Sequence.Control;
using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using SQEX.Ebony.Framework.Sequence.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SequenceEditor.Components
{
    public class DiagramPersistence
    {
        public void Load(Diagram diagram, DiagramData data)
        {
            var idMap = new Dictionary<string, string>();

            foreach (var node in data.Nodes)
            {
                var newNode = new StandardNode(Type.GetType(node.Type), new Point(node.X, node.Y));
                diagram.Nodes.Add(newNode);
                idMap.Add(node.Id, newNode.Id);
            }

            foreach (var link in data.Links)
            {
                var sourceId = idMap[link.SourceId];
                var targetId = idMap[link.TargetId];
                var sourceNode = diagram.Nodes.Single(n => n.Id == sourceId);
                var targetNode = diagram.Nodes.Single(n => n.Id == targetId);
                var sourcePort = sourceNode.Ports.Single(p => (p as StandardPort).Name == link.SourcePortName) as StandardPort;
                var targetPort = targetNode.Ports.Single(p => (p as StandardPort).Name == link.TargetPortName) as StandardPort;
                var newLink = new LinkModel(sourcePort, targetPort);

                var color = sourcePort?.Color ?? targetPort.Color;
                if (color == StandardPort.Control)
                {
                    newLink.TargetMarker = LinkMarker.NewArrow(16, 8);
                    newLink.Width = 4;
                }

                newLink.Color = color;
                diagram.Links.Add(newLink);

                if (sourcePort != null)
                {
                    var temp = sourcePort.Name;
                    sourcePort.Name = "";
                    sourcePort.Name = temp;
                }

                if (targetPort != null)
                {
                    var temp = targetPort.Name;
                    targetPort.Name = "";
                    targetPort.Name = temp;
                }
            }
        }

        public void New(Diagram diagram)
        {
            //var n = new NodeModel(new Point(0, 50));
            //n.AddPort(PortAlignment.Left);
            //n.AddPort(PortAlignment.Right);
            //diagram.Nodes.Add(n);

            //var n2 = new NodeModel(new Point(0, 50));
            //n2.AddPort(PortAlignment.Left);
            //n2.AddPort(PortAlignment.Right);
            //diagram.Nodes.Add(n2);

            //var group = new GroupModel(new List<NodeModel> { n, n2 });
            //diagram.AddGroup(group);
            //var group2 = new GroupModel(new List<NodeModel> { });
            //group2.AddPort(PortAlignment.Left);
            //group2.AddPort(PortAlignment.Right);
            ////diagram.Nodes.Add(group2);
            //diagram.AddGroup(group2);
            //group2.AddChild(group);

            var node = new StandardNode(typeof(SequenceEventSequenceStarted), new Point(300, 50));
            diagram.Nodes.Add(node);
            node = new StandardNode(typeof(SequenceActionControlIf), new Point(300, 50));
            diagram.Nodes.Add(node);
            node = new StandardNode(typeof(SequenceActionEventScriptRegisterActor), new Point(300, 50));
            diagram.Nodes.Add(node);
            node = new StandardNode(typeof(SequenceActionActorCheckTag), new Point(300, 50));
            diagram.Nodes.Add(node);
            node = new StandardNode(typeof(SequenceActionActorGetExeActor), new Point(300, 50));
            diagram.Nodes.Add(node);
            node = new StandardNode(typeof(SequenceActionActorTypeIDSwitch), new Point(600, 50));
            diagram.Nodes.Add(node);
        }
    }
}
