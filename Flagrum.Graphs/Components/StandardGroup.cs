using Blazor.Diagrams.Core.Models;
using SQEX.Ebony.Framework.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SequenceEditor.Components
{
    public class StandardGroup : GroupModel
    {
        public string Title { get; set; } = "Sequence Tray";

        public StandardGroup() : base(new List<NodeModel>(), 50)
        {
            Size = new Blazor.Diagrams.Core.Geometry.Size(400, 300);
            AddPort(new StandardPort(this, PortAlignment.Left, typeof(GraphTriggerInputPin), "In"));
            AddPort(new StandardPort(this, PortAlignment.Right, typeof(GraphTriggerOutputPin), "Out"));
        }
    }
}
