using System.Collections.Generic;
using System.Xml;

namespace Flagrum.Core.Utilities.Extensions;

public static class XmlExtensions
{
    public static int GetIntegerAttribute(this XmlElement element, string name, int defaultValue)
    {
        var attribute = element.Attributes[name];
        return attribute != null && int.TryParse(attribute.Value, out var result) ? result : defaultValue;
    }

    public static bool GetBooleanAttribute(this XmlElement element, string name, bool defaultValue)
    {
        var attribute = element.Attributes[name];
        return attribute != null && bool.TryParse(attribute.Value, out var result) ? result : defaultValue;
    }

    public static string GetStringAttribute(this XmlElement element, string name)
    {
        return element.Attributes[name]?.Value;
    }

    public static List<XmlElement> GetElements(this XmlElement element, string name)
    {
        var list = new List<XmlElement>();
        foreach (XmlNode childNode in element.ChildNodes)
        {
            if (childNode is XmlElement node && node.Name == name)
            {
                list.Add(node);
            }
        }

        return list;
    }
}