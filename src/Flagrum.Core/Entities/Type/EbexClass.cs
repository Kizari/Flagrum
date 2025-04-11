// Decompiled with JetBrains decompiler
// Type: SQEX.Ebony.Jenova.Data.Type.Class
// Assembly: JenovaData, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A121E2BE-2AE2-4B35-86C9-2B1E4661CBF5
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\FFXVModOrganizer\LuminousStudio\luminous\sdk\tools\Backend\AssetConverterFramework\BuildCoordinator\bin\JenovaData.dll

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using Flagrum.Core.Scripting.Ebex.Data;

namespace Flagrum.Core.Scripting.Ebex.Type;

public class Class : DataType
{
    private readonly List<Class> derivedTypeList = new();
    private readonly List<Function> funcs = new();
    private bool containsDynamicDropDownList_;
    private bool containsDynamicLoadItem_;
    private bool containsDynamicLoadPin_;
    private Dictionary<string, string> inheritedAttributes;

    public Class(string name)
        : base(name) { }

    public Class[] BaseTypes { get; private set; }

    public string[] BaseNames { get; private set; }

    public Dictionary<string, string> InheritedAttributes
    {
        get
        {
            lock (this)
            {
                if (inheritedAttributes == null)
                {
                    inheritedAttributes = new Dictionary<string, string>(Attributes);
                    foreach (var baseType in BaseTypes)
                    {
                        foreach (var inheritedAttribute in baseType.InheritedAttributes)
                        {
                            if (!inheritedAttributes.ContainsKey(inheritedAttribute.Key) &&
                                inheritedAttribute.Value != null)
                            {
                                inheritedAttributes[inheritedAttribute.Key] = inheritedAttribute.Value;
                            }
                        }
                    }
                }

                return inheritedAttributes;
            }
        }
    }

    public ReadOnlyCollection<Class> DerivedTypes => derivedTypeList.AsReadOnly();

    public Field this[string name]
    {
        get => Fields.Where(x => x.Name == name).FirstOrDefault();
        set
        {
            for (var index = 0; index < Fields.Length; ++index)
            {
                if (Fields[index].Name == name)
                {
                    Fields[index] = value;
                }
            }
        }
    }

    public Field[] Fields { get; set; } = new Field[0];

    public Field[] ComponentPropertyFields { get; private set; } = new Field[0];

    public IEnumerable<Function> Functions => funcs;

    public bool NoChild
    {
        get
        {
            bool flag;
            return TryGetAttributeBool(nameof(NoChild), out flag) && flag;
        }
    }

    public bool Deprecated
    {
        get
        {
            bool flag;
            return TryGetAttributeBool(nameof(Deprecated), out flag) && flag;
        }
    }

    public string NodeShape => GetAttribute(nameof(NodeShape)) ?? "Rectangle";

    public override int WakeUpCurveEditorByDoubleClick
    {
        get
        {
            int num;
            return TryGetAttributeInt(nameof(WakeUpCurveEditorByDoubleClick), out num) ? num : -1;
        }
    }

    public override bool TimeLine
    {
        get
        {
            bool flag;
            return TryGetAttributeBool(nameof(TimeLine), out flag) && flag;
        }
    }

    public bool IsTimeLineUniqueTrackInGroup
    {
        get
        {
            bool flag;
            if (TryGetAttributeBool(nameof(IsTimeLineUniqueTrackInGroup), out flag))
            {
                return flag;
            }

            foreach (var baseType in BaseTypes)
            {
                var uniqueTrackInGroup = baseType.IsTimeLineUniqueTrackInGroup;
                if (uniqueTrackInGroup)
                {
                    return uniqueTrackInGroup;
                }
            }

            return false;
        }
    }

    public bool IsTimeLineTrackOnly
    {
        get
        {
            bool flag;
            if (TryGetAttributeBool(nameof(IsTimeLineTrackOnly), out flag))
            {
                return flag;
            }

            foreach (var baseType in BaseTypes)
            {
                var timeLineTrackOnly = baseType.IsTimeLineTrackOnly;
                if (timeLineTrackOnly)
                {
                    return timeLineTrackOnly;
                }
            }

            return false;
        }
    }

    public override bool IsTimeLineKeyCreateWhenRunning
    {
        get
        {
            bool flag;
            if (TryGetAttributeBool(nameof(IsTimeLineKeyCreateWhenRunning), out flag))
            {
                return flag;
            }

            foreach (DataType baseType in BaseTypes)
            {
                var createWhenRunning = baseType.IsTimeLineKeyCreateWhenRunning;
                if (createWhenRunning)
                {
                    return createWhenRunning;
                }
            }

            return false;
        }
    }

    public string TimeLineAutoCreateTrackClass => GetAttribute(nameof(TimeLineAutoCreateTrackClass));

    public string TrackItemClass
    {
        get
        {
            var attribute = GetAttribute(nameof(TrackItemClass));
            if (attribute != null)
            {
                return attribute;
            }

            foreach (var baseType in BaseTypes)
            {
                var trackItemClass = baseType.TrackItemClass;
                if (trackItemClass != null)
                {
                    return trackItemClass;
                }
            }

            return null;
        }
    }

    public List<string> TimeLineCurveUniqueNameList
    {
        get
        {
            var attribute = GetAttribute("TimeLineCurveUniqueName");
            if (attribute == null)
            {
                return null;
            }

            var separator = new string[1] {","};
            var strArray = attribute.Split(separator, StringSplitOptions.None);
            var stringList = new List<string>();
            foreach (var str in strArray)
            {
                stringList.Add(str);
            }

            return stringList;
        }
    }

    public string TrackElementClass
    {
        get
        {
            var attribute = GetAttribute(nameof(TrackElementClass));
            if (attribute != null)
            {
                return attribute;
            }

            foreach (var baseType in BaseTypes)
            {
                var trackElementClass = baseType.TrackElementClass;
                if (trackElementClass != null)
                {
                    return trackElementClass;
                }
            }

            return null;
        }
    }

    public string[] TrackElementClassList
    {
        get
        {
            var trackElementClass = TrackElementClass;
            if (trackElementClass == null)
            {
                return null;
            }

            var separator = new string[1] {","};
            return trackElementClass.Split(separator, StringSplitOptions.None);
        }
    }

    public string TimeLineUserGroupNamePrefix => GetAttribute(nameof(TimeLineUserGroupNamePrefix));

    public string TimeLineTrackDetailType => GetAttribute(nameof(TimeLineTrackDetailType));

    public string TimeLineTrackCurveType => GetAttribute(nameof(TimeLineTrackCurveType));

    public string TimeLineTrackItemType => GetAttribute(nameof(TimeLineTrackItemType));

    public string TimeLineGroupType => GetAttribute(nameof(TimeLineGroupType)) ?? "GT_NONE";

    public string TimeLineTrackType => GetAttribute(nameof(TimeLineTrackType));

    public string LaunchTool => GetAttributeRecursive(nameof(LaunchTool));

    public string LaunchItem => GetAttributeRecursive(nameof(LaunchItem));

    public override string IconName
    {
        get
        {
            var iconName1 = base.IconName;
            if (iconName1 != null)
            {
                return iconName1;
            }

            foreach (DataType baseType in BaseTypes)
            {
                var iconName2 = baseType.IconName;
                if (iconName2 != null)
                {
                    return iconName2;
                }
            }

            return null;
        }
    }

    public override string Category
    {
        get
        {
            var category1 = base.Category;
            if (category1 != null)
            {
                return category1;
            }

            foreach (DataType baseType in BaseTypes)
            {
                var category2 = baseType.Category;
                if (category2 != null)
                {
                    return category2;
                }
            }

            return null;
        }
    }

    public string SequenceClass => GetAttributeRecursive(nameof(SequenceClass));

    public string DefaultName => GetAttribute(nameof(DefaultName));

    public bool ContainsDynamicLoadItem
    {
        get
        {
            var flag = containsDynamicLoadItem_;
            if (!flag)
            {
                foreach (var baseType in BaseTypes)
                {
                    if (baseType.ContainsDynamicLoadItem)
                    {
                        flag = true;
                        break;
                    }
                }
            }

            return flag;
        }
        set => containsDynamicLoadItem_ = true;
    }

    public bool ContainsDynamicLoadPin
    {
        get
        {
            var flag = containsDynamicLoadPin_;
            if (!flag)
            {
                foreach (var baseType in BaseTypes)
                {
                    if (baseType.ContainsDynamicLoadPin)
                    {
                        flag = true;
                        break;
                    }
                }
            }

            return flag;
        }
        set => containsDynamicLoadPin_ = true;
    }

    public bool ContainsDynamicDropDownList
    {
        get
        {
            var flag = containsDynamicDropDownList_;
            if (!flag)
            {
                foreach (var baseType in BaseTypes)
                {
                    if (baseType.containsDynamicDropDownList_)
                    {
                        flag = true;
                        break;
                    }
                }
            }

            return flag;
        }
        set => containsDynamicDropDownList_ = true;
    }

    public string GetAttributeRecursive(string name)
    {
        var inheritedAttributes = InheritedAttributes;
        string str;
        return (DocumentInterface.Configuration.JapaneseLanguage &&
                inheritedAttributes.TryGetValue(name + "JP", out str)) || inheritedAttributes.TryGetValue(name, out str)
            ? str
            : null;
    }

    public IEnumerable<string> GetAttributeRecursiveAll(
        string name,
        ref List<string> list)
    {
        var attribute = GetAttribute(name);
        if (attribute != null)
        {
            list.Add(attribute);
        }

        foreach (var baseType in BaseTypes)
        {
            baseType.GetAttributeRecursiveAll(name, ref list);
        }

        return list;
    }

    public void GetAllField(ref List<Field> list)
    {
        foreach (var field in Fields)
        {
            list.Add(field);
        }

        foreach (var baseType in BaseTypes)
        {
            baseType.GetAllField(ref list);
        }
    }

    public Field GetFieldFromAllBase(string name)
    {
        var list = new List<Field>();
        GetAllField(ref list);
        foreach (var field in list)
        {
            if (field.Name == name)
            {
                return field;
            }
        }

        return null;
    }

    public bool IsBasedOn(Class baseType)
    {
        if (baseType == null)
        {
            return false;
        }

        return this == baseType || BaseTypes.Where(b => b.IsBasedOn(baseType)).Count() > 0;
    }

    public string GetDefaultValueOverride(string findName)
    {
        var attributeRecursive = GetAttributeRecursive("DefaultValueOverride");
        if (attributeRecursive != null && attributeRecursive.Contains(findName))
        {
            var str1 = attributeRecursive;
            var chArray1 = new char[1] {';'};
            foreach (var str2 in str1.Split(chArray1))
            {
                var chArray2 = new char[1] {'/'};
                var strArray = str2.Split(chArray2);
                if (strArray.Length == 2)
                {
                    var str3 = strArray[0];
                    var str4 = strArray[1];
                    if (str3 == findName)
                    {
                        return str4;
                    }
                }
            }
        }

        return null;
    }

    public void loadXml(XmlElement element)
    {
        addBaseClasses(XmlUtility.GetElements(element, "baseclasses", "baseclass"));
        addFields(XmlUtility.GetElements(element, "properties", "property"));
        addFunctions(XmlUtility.GetElements(element, "functions", "function"));
        addAttrbitues(XmlUtility.GetElements(element, "attributes", "attribute"));
    }

    public void addBaseClasses(XmlElement[] elements)
    {
        var stringList = new List<string>();
        foreach (var element in elements)
        {
            var attributeText = XmlUtility.GetAttributeText(element, "type");
            if (attributeText != null)
            {
                stringList.Add(attributeText);
            }
        }

        BaseNames = stringList.ToArray();
    }

    public void addFunctions(XmlElement[] elements)
    {
        foreach (var element1 in elements)
        {
            var attributeText1 = XmlUtility.GetAttributeText(element1, "name");
            var attributeBool1 = XmlUtility.GetAttributeBool(element1, "static", false);
            var attributeText2 = XmlUtility.GetAttributeText(element1, "returntype");
            var attributeBool2 = XmlUtility.GetAttributeBool(element1, "returntype_pointer", false);
            var returnTypeName = attributeText2;
            var num1 = attributeBool1 ? 1 : 0;
            var num2 = attributeBool2 ? 1 : 0;
            var function = new Function(attributeText1, returnTypeName, num1 != 0, num2 != 0);
            function.addAttrbitues(XmlUtility.GetElements(element1, "attributes", "attribute"));
            var elements1 = XmlUtility.GetElements(element1, "argument");
            var num3 = 0;
            foreach (var element2 in elements1)
            {
                var obj = new Argument(XmlUtility.GetAttributeText(element2, "name"),
                    XmlUtility.GetAttributeText(element2, "type"),
                    XmlUtility.GetAttributeBool(element2, "pointer", false));
                var attribute1 = function.GetAttribute("DisplayNameArg" + num3, false);
                var attribute2 = function.GetAttribute("DisplayNameArg" + num3, true);
                obj.SetDisplayName(attribute1, attribute2);
                function.AddArgument(obj);
                ++num3;
            }

            funcs.Add(function);
        }
    }

    public void addFields(XmlElement[] elements)
    {
        var fieldList1 = new List<Field>();
        var fieldList2 = new List<Field>();
        foreach (var element in elements)
        {
            var attributeText1 = XmlUtility.GetAttributeText(element, "name");
            var attributeText2 = XmlUtility.GetAttributeText(element, "type");
            var attributeBool = XmlUtility.GetAttributeBool(element, "pointer", false);
            if (attributeText1 != null && attributeText2 != null)
            {
                var field = new Field(attributeText1, attributeText2, attributeBool);
                field.addAttrbitues(XmlUtility.GetElements(element, "attributes", "attribute"));
                fieldList1.Add(field);
                var flag = false;
                if (field.TryGetAttributeBool("ComponentProperty", out flag))
                {
                    fieldList2.Add(field);
                }
            }
        }

        Fields = fieldList1.ToArray();
        ComponentPropertyFields = fieldList2.ToArray();
    }

    public override void matchTypes(ModuleContainer moduleContainer)
    {
        base.matchTypes(moduleContainer);
        var classList = new List<Class>();
        foreach (var baseName in BaseNames)
        {
            if (moduleContainer[baseName] is Class class1)
            {
                classList.Add(class1);
                class1.derivedTypeList.Add(this);
            }
        }

        BaseTypes = classList.ToArray();
        foreach (DataType field in Fields)
        {
            field.matchTypes(moduleContainer);
        }

        foreach (DataType function in Functions)
        {
            function.matchTypes(moduleContainer);
        }
    }
}