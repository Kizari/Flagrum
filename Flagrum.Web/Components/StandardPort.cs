using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using SQEX.Ebony.Framework.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Flagrum.Web.Components
{
    public class StandardPort : PortModel
    {
        public const string Control = "#ccffff";
        public const string Rose = "#008080";

        public StandardPort(NodeModel parent, PortAlignment alignment, Type type, string name) : base(parent, alignment)
        {
            Type = type;
            Name = name;
            Color = GetColorByType();
        }

        public Dictionary<Type, string> TypeColors { get; set; } = new()
        {
            {typeof(GraphInputPin), Control},
            {typeof(GraphOutputPin), Control},
            {typeof(GraphTriggerInputPin), Control},
            {typeof(GraphTriggerOutputPin), Control},
            {typeof(GraphVariableInputPin), Rose},
            {typeof(GraphVariableOutputPin), Rose}
        };

        public Type Type { get; set; }
        public string Color { get; set; }
        public string Name { get; set; }

        public override bool CanAttachTo(PortModel port)
        {
            return true;
        }

        private string GetColorByType()
        {
            if (TypeColors.TryGetValue(Type, out string color))
            {
                return color;
            }
            else
            {
                return "grey";
            }
        }
    }
}