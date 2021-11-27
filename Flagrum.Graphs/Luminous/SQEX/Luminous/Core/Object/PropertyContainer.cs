using System.Collections.Generic;

namespace SQEX.Luminous.Core.Object
{
    public class PropertyContainer
    {
        private string TypeName { get; }
        private int NameHashCode { get; }
        private int VersionHashCode { get; }
        private ushort AllPropertiesClassFieldCount { get; set; }
        private PropertyContainer Parent { get; }
        private IList<Property> MyProperties { get; } = new List<Property>();
        private IList<Property> AllProperties { get; } = new List<Property>();

        public PropertyContainer(string name, PropertyContainer parent, int nameHashCode, int versionHashCode)
        {
            this.TypeName = name;
            this.NameHashCode = nameHashCode;
            this.VersionHashCode = versionHashCode;
            this.Parent = parent;
        }

        public Property FindByName(string name)
        {
            foreach(var property in this.AllProperties)
            {
                if (property.Name == name)
                {
                    return property;
                }
            }

            // TODO: This shouldn't be necessary, remove
            var parent = this.Parent;
            while(parent != null)
            {
                var property = parent.FindByName(name);
                if (property != null)
                {
                    return property;
                }

                parent = parent.Parent;
            }

            return null;
        }

        public void AddProperty(Property property)
        {
            this.MyProperties.Add(property);
            this.AllProperties.Add(property);

            if (property.Type == Property.PrimitiveType.ClassField)
            {
                this.AllPropertiesClassFieldCount++;
            }
        }

        public void AddIndirectlyProperty(Property property)
        {
            this.AllProperties.Add(property);

            if (property.Type == Property.PrimitiveType.ClassField)
            {
                this.AllPropertiesClassFieldCount++;
            }
        }
    }
}