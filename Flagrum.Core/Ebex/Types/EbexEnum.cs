using System.Collections.Generic;
using System.Xml;
using Flagrum.Core.Utilities;

namespace Flagrum.Core.Ebex.Types;

public class EbexEnum : EbexType
{
    public EbexEnum(XmlElement element)
    {
        Name = element.GetAttribute("name");

        foreach (var item in element.GetChildContainerChildren("items", "item"))
        {
            Items.Add(new EbexEnumItem(item));
        }

        ReadAttributes(element.GetChildContainerChildren("attributes", "attribute"));
    }

    public List<EbexEnumItem> Items { get; set; } = new();
}