using System;
using System.Collections.Generic;
using System.Linq;
using Black.Sequence.Action.EventScript;
using Black.Sequence.Actor;
using Black.Sequence.Control;
using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using SQEX.Ebony.Framework.Sequence.Event;

namespace Flagrum.Web.Components.Graph;

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
            var sourcePort = sourceNode.Ports
                .FirstOrDefault(p => (p as StandardPort)?.Name == link.SourcePortName) as StandardPort;
            var targetPort = targetNode.Ports
                .FirstOrDefault(p => (p as StandardPort)?.Name == link.TargetPortName) as StandardPort;
            var newLink = new LinkModel(sourcePort!, targetPort);

            var color = sourcePort?.Color ?? targetPort.Color;
            if (color == StandardPort.Control)
            {
                newLink.TargetMarker = LinkMarker.NewArrow(16, 8);
                newLink.Width = 4;
            }

            newLink.Color = color;
            diagram.Links.Add(newLink);

            // if (sourcePort != null)
            // {
            //     var temp = sourcePort.Name;
            //     sourcePort.Name = "";
            //     sourcePort.Name = temp;
            // }
            //
            // if (targetPort != null)
            // {
            //     var temp = targetPort.Name;
            //     targetPort.Name = "";
            //     targetPort.Name = temp;
            // }
        }
    }

    public void New(Diagram diagram)
    {
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