using System;
using System.Collections.Generic;
using System.Xml;
using Flagrum.Core.Utilities;

namespace Flagrum.Core.Ebex.Types;

public class EbexFunction : EbexType
{
    public EbexFunction(XmlElement element)
    {
        Name = element.GetAttribute("name");
        IsStatic = element.GetAttribute("static").Equals("true", StringComparison.OrdinalIgnoreCase);
        ReturnType = element.GetAttribute("returntype");
        IsReturnTypePointer = element.GetAttribute("returntype_pointer")
            .Equals("true", StringComparison.OrdinalIgnoreCase);

        ReadAttributes(element.GetChildContainerChildren("attributes", "attribute"));

        foreach (var argumentObject in element.GetElementsByTagName("argument"))
        {
            if (argumentObject is XmlElement argument)
            {
                Arguments.Add(new EbexFunctionArgument(argument));
            }
        }
    }

    public bool IsStatic { get; set; }
    public string ReturnType { get; set; }
    public bool IsReturnTypePointer { get; set; }
    public List<EbexFunctionArgument> Arguments { get; set; } = new();
}