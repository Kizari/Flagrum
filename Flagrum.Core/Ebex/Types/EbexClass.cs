using System.Collections.Generic;
using System.Xml;
using Flagrum.Core.Utilities;

namespace Flagrum.Core.Ebex.Types;

public class EbexClass : EbexType
{
    public EbexClass(XmlElement element)
    {
        Name = element.GetAttribute("name");
        ReadAttributes(element);
        ReadBaseClasses(element);
        ReadFunctions(element);
        ReadProperties(element);
    }

    public List<string> BaseClassNames { get; set; } = new();
    public List<EbexFunction> Functions { get; set; } = new();
    public List<EbexProperty> Properties { get; set; } = new();
    public List<EbexProperty> ComponentProperties { get; set; } = new();

    private void ReadAttributes(XmlElement element)
    {
        var attributes = element.GetChildContainerChildren("attributes", "attribute");
        foreach (var attribute in attributes)
        {
            var name = attribute.GetAttribute("name");

            if (!Attributes.ContainsKey(name))
            {
                Attributes.Add(name, element.InnerText);
            }
        }
    }

    private void ReadBaseClasses(XmlElement element)
    {
        var baseClasses = element.GetChildContainerChildren("baseclasses", "baseclass");
        foreach (var baseClass in baseClasses)
        {
            BaseClassNames.Add(baseClass.GetAttribute("type"));
        }
    }

    private void ReadFunctions(XmlElement element)
    {
        var functions = element.GetChildContainerChildren("functions", "function");
        foreach (var function in functions)
        {
            Functions.Add(new EbexFunction(function));
        }
    }

    private void ReadProperties(XmlElement element)
    {
        var properties = element.GetChildContainerChildren("properties", "property");
        foreach (var property in properties)
        {
            var ebexProperty = new EbexProperty(property, out var isComponentProperty);
            if (isComponentProperty)
            {
                ComponentProperties.Add(ebexProperty);
            }
            else
            {
                Properties.Add(ebexProperty);
            }
        }
    }
}