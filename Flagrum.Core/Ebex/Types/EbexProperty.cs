using System;
using System.Xml;
using Flagrum.Core.Utilities;

namespace Flagrum.Core.Ebex.Types;

public class EbexProperty : EbexType
{
    public EbexProperty(XmlElement element, out bool isComponentProperty)
    {
        Name = element.GetAttribute("name");
        Type = element.GetAttribute("type");
        IsPointer = element.GetAttribute("pointer").Equals("true", StringComparison.OrdinalIgnoreCase);

        ReadAttributes(element.GetChildContainerChildren("attributes", "attribute"));

        isComponentProperty = GetBool("ComponentProperty");
    }

    public string Name { get; set; }
    public string Type { get; set; }
    public bool IsPointer { get; set; }
}