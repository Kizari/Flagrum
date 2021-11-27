using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SequenceEditor.Layout
{
    public class TypeTree
    {
        public string Name { get; set; }
        public bool IsCollapsed { get; set; } = true;
        public bool IsVisible { get; set; } = true;
        public Type Type { get; set; }
        public List<TypeTree> Children { get; set; } = new();
    }
}
