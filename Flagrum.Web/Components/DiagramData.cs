using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Flagrum.Web.Components
{
    public class DiagramData
    {
        public IEnumerable<NodeData> Nodes { get; set; }
        public IEnumerable<LinkData> Links { get; set; }

        public static DiagramData FromDiagram(Diagram diagram)
        {
            var data = new DiagramData();

            data.Nodes = diagram.Nodes.Select(n => new NodeData
            {
                Id = n.Id,
                X = n.Position.X,
                Y = n.Position.Y,
                Type = (n as StandardNode).Type.AssemblyQualifiedName
            });

            data.Links = diagram.Links.Select(l => new LinkData
            {
                SourceId = l.SourceNode?.Id,
                TargetId = l.TargetNode?.Id,
                SourcePortName = (l.SourcePort as StandardPort)?.Name,
                TargetPortName = (l.TargetPort as StandardPort)?.Name
            });

            return data;
        }
    }

    public class NodeData
    {
        public string Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public string Type { get; set; }
    }

    public class LinkData
    {
        public string SourceId { get; set; }
        public string TargetId { get; set; }
        public string SourcePortName { get; set; }
        public string TargetPortName { get; set; }
    }
}