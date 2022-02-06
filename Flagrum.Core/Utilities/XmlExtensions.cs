using System.Collections.Generic;
using System.Xml;

namespace Flagrum.Core.Utilities;

public static class XmlExtensions
{
    public static IEnumerable<XmlElement> GetChildContainerChildren(this XmlElement element, string container,
        string elementName)
    {
        foreach (var child in element.ChildNodes)
        {
            if (child is XmlElement childElement && childElement.Name == container)
            {
                foreach (var subchild in childElement.ChildNodes)
                {
                    if (subchild is XmlElement subchildElement && subchildElement.Name == elementName)
                    {
                        yield return subchildElement;
                    }
                }
            }
        }
    }
}