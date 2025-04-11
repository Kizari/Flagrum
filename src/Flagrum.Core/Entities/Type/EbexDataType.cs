using System.Collections.Generic;
using System.Text;
using System.Xml;
using Flagrum.Core.Scripting.Ebex.Data;

namespace Flagrum.Core.Scripting.Ebex.Type;

public class DataType
{
    public delegate DataItem InstanciatorDelegate(DataItem parent, DataType dataType);

    private readonly List<string> attributesOrder = new();

    public DataType(string name)
    {
        Name = name;
        Instanciator = null;
    }

    public string Name { get; }

    public string ShortName
    {
        get
        {
            var fullName = Name;
            var num = Name.LastIndexOf('.');
            return num >= 0 ? Name.Substring(num + 1) : Name;
        }
    }

    public string DisplayName
    {
        get => getDisplayName(DocumentInterface.Configuration.JapaneseLanguage);
        set => Attributes[nameof(DisplayName)] = value;
    }

    public string DisplayNameJP => getDisplayName(true);

    public string DisplayNameEN => getDisplayName(false);

    public string FullName => Name;

    public string Description => GetAttribute(nameof(Description)) ?? GetAttribute("DescriptionJP");

    public virtual string Category
    {
        get
        {
            var str = GetAttribute(nameof(Category));
            if (str != null)
            {
                str = str.Replace('/', '.');
            }

            return str;
        }
    }

    public virtual bool ReadOnly
    {
        get
        {
            bool flag;
            return TryGetAttributeBool(nameof(ReadOnly), out flag) && flag;
        }
        set { }
    }

    public string DefaultValueString => GetAttribute("DefaultValue");

    public virtual IItem DefaultValue => null;

    public virtual bool Browsable
    {
        get
        {
            bool flag;
            return !TryGetAttributeBool(nameof(Browsable), out flag) || flag;
        }
    }

    public virtual int WakeUpCurveEditorByDoubleClick => -1;

    public virtual bool TimeLine => false;

    public virtual bool IsTimeLineKeyCreateWhenRunning => false;

    public virtual bool DataGrid => false;

    public virtual string IconName => GetAttribute("Icon");

    public Dictionary<string, string> Attributes { get; set; } = new();

    public IEnumerable<string> AttributesOrder
    {
        get
        {
            foreach (var str in attributesOrder)
            {
                yield return str;
            }
        }
    }

    public InstanciatorDelegate Instanciator { get; set; }

    public bool IsInvalid { get; private set; }

    private string getDisplayName(bool isJapaneseLanguage)
    {
        var attribute = GetAttribute("DisplayName", isJapaneseLanguage);
        if (attribute != null && (isJapaneseLanguage ||
                                  Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(attribute)) == attribute))
        {
            return attribute;
        }

        var str = ShortName;
        if (str.Length > 0 && char.IsLower(str[0]))
        {
            str = char.ToUpper(str[0]) + str.Substring(1);
        }

        if (str.EndsWith("_"))
        {
            str = str.Substring(0, str.Length - 1);
        }

        return str;
    }

    public bool ExistAttribute(string name)
    {
        return Attributes.ContainsKey(name);
    }

    public string GetAttribute(string name, bool isJapaneseLanguage)
    {
        return Attributes.TryGetValue(name, out var displayName)
            ? string.IsNullOrEmpty(displayName)
                ? Attributes.TryGetValue(name + "JP", out var jpDisplayName)
                    ? jpDisplayName
                    : null
                : displayName
            : Attributes.TryGetValue(name + "JP", out var jpDisplayName2)
                ? jpDisplayName2
                : null;

        string str;
        return (isJapaneseLanguage && Attributes.TryGetValue(name + "JP", out str)) ||
               Attributes.TryGetValue(name, out str)
            ? str
            : null;
    }

    public string GetAttribute(string name)
    {
        return GetAttribute(name, DocumentInterface.Configuration.JapaneseLanguage);
    }

    public bool TryGetAttributeInt(string name, out int value)
    {
        string s;
        if (Attributes.TryGetValue(name, out s))
        {
            return int.TryParse(s, out value);
        }

        value = -1;
        return false;
    }

    public bool TryGetAttributeFloat(string name, out float value)
    {
        string s;
        if (Attributes.TryGetValue(name, out s))
        {
            return float.TryParse(s, out value);
        }

        value = -1f;
        return false;
    }

    public bool TryGetAttributeDouble(string name, out double value)
    {
        string s;
        if (Attributes.TryGetValue(name, out s))
        {
            return double.TryParse(s, out value);
        }

        value = 0.0;
        return false;
    }

    public bool TryGetAttributeBool(string name, out bool value)
    {
        string str;
        if (Attributes.TryGetValue(name, out str))
        {
            return bool.TryParse(str, out value);
        }

        value = false;
        return false;
    }

    public bool TryGetAttributeString(string name, out string value)
    {
        if (Attributes.TryGetValue(name, out value))
        {
            return true;
        }

        value = "";
        return false;
    }

    public void RemoveAttrbitue(string name)
    {
        Attributes.Remove(name);
        attributesOrder.Remove(name);
    }

    public void AddAttribute(string name, string value)
    {
        Attributes.Add(name, value);
        attributesOrder.Add(name);
    }

    public void addAttrbitues(XmlElement[] elements)
    {
        Attributes.Clear();
        foreach (var element in elements)
        {
            var attributeText = XmlUtility.GetAttributeText(element, "name");
            var innerText = element.InnerText;
            if (attributeText != null && innerText != null && !Attributes.ContainsKey(attributeText))
            {
                Attributes.Add(attributeText, innerText);
                attributesOrder.Add(attributeText);
            }
        }
    }

    public virtual void matchTypes(ModuleContainer moduleContainer)
    {
    }

    public DataItem CreateDataItem(DataItem parent)
    {
        if (Instanciator != null)
        {
            return Instanciator(parent, this);
        }

        if (this is Field)
        {
            return new FieldDataItem(parent, (Field) this);
        }

        return PrimitiveTypeUtility.FromNameSimple(Name) != PrimitiveType.None
            ? new ValueDataItem(parent, this)
            : new UnknownDataItem(parent, this);
    }

    public DataItem CreateIItem(DataItem parent)
    {
        return Instanciator != null ? Instanciator(parent, this) : null;
    }

    public void invalidate()
    {
        IsInvalid = true;
    }
}