using System.Collections.Generic;

namespace SQEX.Luminous.Core.Object;

public class PropertyContainer
{
    public PropertyContainer(string name, PropertyContainer parent, int nameHashCode, int versionHashCode)
    {
        TypeName = name;
        NameHashCode = nameHashCode;
        VersionHashCode = versionHashCode;
        Parent = parent;
    }

    private string TypeName { get; }
    private int NameHashCode { get; }
    private int VersionHashCode { get; }
    private ushort AllPropertiesClassFieldCount { get; set; }
    private PropertyContainer Parent { get; }
    private IList<Property> MyProperties { get; } = new List<Property>();
    public IList<Property> AllProperties { get; } = new List<Property>();

    public Property FindByName(string name)
    {
        foreach (var property in AllProperties)
        {
            if (property.Name == name)
            {
                return property;
            }
        }

        // TODO: This shouldn't be necessary, remove
        var parent = Parent;
        while (parent != null)
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
        MyProperties.Add(property);
        AllProperties.Add(property);

        if (property.Type == Property.PrimitiveType.ClassField)
        {
            AllPropertiesClassFieldCount++;
        }
    }

    public void AddIndirectlyProperty(Property property)
    {
        AllProperties.Add(property);

        if (property.Type == Property.PrimitiveType.ClassField)
        {
            AllPropertiesClassFieldCount++;
        }
    }
}