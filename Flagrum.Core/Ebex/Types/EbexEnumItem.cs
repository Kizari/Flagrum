using System.Xml;
using Flagrum.Core.Utilities;

namespace Flagrum.Core.Ebex.Types;

public class EbexEnumItem : EbexType
{
    public EbexEnumItem(XmlElement element)
    {
        Name = element.GetAttribute("name");
        ReadAttributes(element.GetChildContainerChildren("attributes", "attribute"));
        Value = GetInt("Value");
    }

    public int Value { get; set; }
}