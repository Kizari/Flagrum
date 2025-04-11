using System.Xml;

namespace Flagrum.Core.Scripting.Ebex.Type;

public class EnumItem : DataType
{
    private int value;

    public EnumItem(string name)
        : base(name) { }

    public int Value => value;

    public void loadXml(XmlElement element)
    {
        addAttrbitues(XmlUtility.GetElements(element, "attributes", "attribute"));
        TryGetAttributeInt("Value", out value);
    }
}