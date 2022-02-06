using System;
using System.Xml;

namespace Flagrum.Core.Ebex.Types;

public class EbexTypedef : EbexType
{
    public EbexTypedef(XmlElement element)
    {
        Name = element.GetAttribute("name");
        Type = element.GetAttribute("type");
        IsPointer = element.GetAttribute("pointer").Equals("true", StringComparison.OrdinalIgnoreCase);
        IsReference = element.GetAttribute("reference").Equals("true", StringComparison.OrdinalIgnoreCase);
    }

    public string Type { get; set; }
    public bool IsPointer { get; set; }
    public bool IsReference { get; set; }
}