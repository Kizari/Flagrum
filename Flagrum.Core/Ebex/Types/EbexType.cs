using System;
using System.Collections.Generic;
using System.Xml;
using Flagrum.Core.Ebex.Entities;

namespace Flagrum.Core.Ebex.Types;

public class EbexType
{
    public string Name { get; set; }
    public Dictionary<string, string> Attributes { get; set; }
    public Func<EbexElement, EbexType, EbexElement> Instantiate { get; set; }

    public EbexElement InstantiateElement(EbexElement parent)
    {
        return Instantiate?.Invoke(parent, this);
    }

    public EbexElement BuildElement(EbexElement parent)
    {
        if (Instantiate != null)
        {
            return Instantiate(parent, this);
        }

        if (this is EbexProperty property)
        {
            return new EbexPropertyElement(parent, property);
        }

        return PrimitiveTypeHelper.FromString(Name) != PrimitiveType.None
            ? new EbexValueElement(parent, this)
            : new UnknownEbexElement(parent, this);
    }

    public void ReadAttributes(IEnumerable<XmlElement> elements)
    {
        foreach (var element in elements)
        {
            var name = element.GetAttribute("name");
            Attributes.Add(name, element.InnerText);
        }
    }

    public bool GetBool(string attributeName)
    {
        return Attributes[attributeName].Equals("true", StringComparison.OrdinalIgnoreCase);
    }

    public int GetInt(string attributeName)
    {
        return Convert.ToInt32(Attributes[attributeName]);
    }
}