using System.Collections.Generic;
using System.Xml;

namespace Flagrum.Core.Scripting.Ebex.Type;

public class Enum : DataType
{
    public Enum(string name)
        : base(name) { }

    public EnumItem[] EnumItems { get; set; }

    public EnumItem DefaultEnumItem => EnumItems.Length != 0 ? EnumItems[0] : null;

    public EnumItem Find(string enumName)
    {
        if (enumName != null && EnumItems != null)
        {
            foreach (var enumItem in EnumItems)
            {
                if (enumItem.Name == enumName)
                {
                    return enumItem;
                }
            }
        }

        return null;
    }

    public EnumItem FindFromDisplayName(string displayName)
    {
        if (displayName != null && EnumItems != null)
        {
            foreach (var enumItem in EnumItems)
            {
                if (enumItem.DisplayName == displayName)
                {
                    return enumItem;
                }
            }
        }

        return null;
    }

    public int ValueOf(string enumName)
    {
        var enumItem = Find(enumName);
        return enumItem != null ? enumItem.Value : 0;
    }

    public string NameOf(int value)
    {
        foreach (var enumItem in EnumItems)
        {
            if (enumItem.Value == value)
            {
                return enumItem.Name;
            }
        }

        return "";
    }

    public bool Contains(string enumName)
    {
        return Find(enumName) != null;
    }

    public void loadXml(XmlElement element)
    {
        var enumItemList = new List<EnumItem>();
        foreach (var element1 in XmlUtility.GetElements(element, "items", "item"))
        {
            var attributeText = XmlUtility.GetAttributeText(element1, "name");
            if (attributeText != null)
            {
                var enumItem = new EnumItem(attributeText);
                enumItem.loadXml(element1);
                enumItemList.Add(enumItem);
            }
        }

        EnumItems = enumItemList.ToArray();
        addAttrbitues(XmlUtility.GetElements(element, "attributes", "attribute"));
    }
}