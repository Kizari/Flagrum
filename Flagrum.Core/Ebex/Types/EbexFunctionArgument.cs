using System;
using System.Xml;

namespace Flagrum.Core.Ebex.Types;

public class EbexFunctionArgument
{
    public EbexFunctionArgument(XmlElement element)
    {
        Name = element.GetAttribute("name");
        Type = element.GetAttribute("type");
        IsPointer = element.GetAttribute("pointer").Equals("true", StringComparison.OrdinalIgnoreCase);
    }

    public string Name { get; set; }
    public string Type { get; set; }
    public bool IsPointer { get; set; }
}