using System.Collections.Generic;
using Blazor.Diagrams.Core.Models;
using SQEX.Ebony.Framework.Node;

namespace Flagrum.Web.Components.Graph
{
    public class StandardGroup : GroupModel
    {
        public StandardGroup() : base(new List<NodeModel>(), 50)
        {
            Size = new Blazor.Diagrams.Core.Geometry.Size(400, 300);
            AddPort(new StandardPort(this, PortAlignment.Left, typeof(GraphTriggerInputPin), "In"));
            AddPort(new StandardPort(this, PortAlignment.Right, typeof(GraphTriggerOutputPin), "Out"));
        }

        public string Title { get; set; } = "Sequence Tray";
    }
}