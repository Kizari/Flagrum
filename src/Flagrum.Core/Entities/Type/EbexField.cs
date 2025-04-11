using System;
using System.Collections.Generic;
using System.Linq;
using Flagrum.Core.Scripting.Ebex.Data;

namespace Flagrum.Core.Scripting.Ebex.Type;

public class Field : DataType
{
    public enum AutoExpandPropertyState
    {
        AEPS_UNKNOWN,
        AEPS_EXPAND,
        AEPS_COLLAPSE
    }

    private readonly string[] templateTypeNames;

    private List<BoollinkStruct> boolLinkList_;
    private IItem defaultValue;
    private List<EnumlinkStruct> enumLinkList_;
    private List<EnumlinkStruct> enumReadOnlyLinkList_;
    private IntlinkStruct intlinkData_;
    private List<ObjectApplicationKey> objectKeyAutoExpandPropertyList_;
    private List<ObjectApplicationKey> objectKeyBrowsableList_;
    private string readOnlylinkData_;

    public Field(string name, string typeName, bool pointer)
        : base(name)
    {
        Pointer = pointer;
        var length = typeName.IndexOf('<');
        var num = typeName.LastIndexOf('>');
        if (length >= 0 && num > length)
        {
            var str1 = typeName.Substring(length + 1, num - length - 1).Trim();
            var strArray = str1.Split(',');
            var stringList = new List<string>();
            if (strArray != null)
            {
                foreach (var str2 in strArray)
                {
                    stringList.Add(str2.Trim());
                }
            }
            else
            {
                stringList.Add(str1);
            }

            templateTypeNames = stringList.ToArray();
            typeName = typeName.Substring(0, length).Trim();
        }

        TypeName = typeName;
        AutoSetPropertyNameList = new List<string>();
    }

    public string[] Renameds
    {
        get
        {
            var attribute = GetAttribute("Renamed");
            string[] strArray;
            if (attribute == null)
            {
                strArray = null;
            }
            else
            {
                strArray = attribute.Split(new string[1] {";"}, StringSplitOptions.RemoveEmptyEntries);
            }

            return strArray ?? new string[0];
        }
    }

    public string TypeName { get; }

    public DataType DataType { get; private set; }

    public Class Class => DataType as Class;

    public bool Pointer { get; }

    public string[] TemplateTypeNames => templateTypeNames ?? new string[0];

    public PrimitiveType FirstTemplatePrimitiveType => templateTypeNames.Length != 0
        ? PrimitiveTypeUtility.FromName(templateTypeNames[0])
        : PrimitiveType.None;

    public bool HasTemplate => templateTypeNames != null && (uint)templateTypeNames.Length > 0U;

    public override string Category => base.Category;

    public string SpecialType => GetAttribute(nameof(SpecialType));

    public string PinValueType => GetAttribute(nameof(PinValueType));

    public string SearchTarget => GetAttribute(nameof(SearchTarget));

    public string CenterTextCommand => GetAttribute(nameof(CenterTextCommand));

    public bool Transient
    {
        get
        {
            bool flag;
            return TryGetAttributeBool(nameof(Transient), out flag) && flag;
        }
    }

    public int MaxConnection
    {
        get
        {
            var num = -1;
            TryGetAttributeInt(nameof(MaxConnection), out num);
            return num;
        }
    }

    public override bool Browsable =>
        (ExistAttribute(nameof(Browsable)) || DataType == null || DataType.Browsable) && base.Browsable;

    public bool Deprecated
    {
        get
        {
            bool flag;
            return TryGetAttributeBool(nameof(Deprecated), out flag) && flag;
        }
    }

    public override bool ReadOnly
    {
        get
        {
            bool flag;
            return TryGetAttributeBool(nameof(ReadOnly), out flag) && flag;
        }
        set => Attributes[nameof(ReadOnly)] = value.ToString();
    }

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

    public bool TimeLineExposable
    {
        get
        {
            bool flag;
            return TryGetAttributeBool(nameof(TimeLineExposable), out flag) && flag;
        }
    }

    public override bool DataGrid
    {
        get
        {
            bool flag;
            return TryGetAttributeBool(nameof(DataGrid), out flag) && flag;
        }
    }

    public bool DependencyPath
    {
        get
        {
            bool flag;
            return TryGetAttributeBool(nameof(DependencyPath), out flag) && flag;
        }
    }

    public bool DependencyFolderPath
    {
        get
        {
            bool flag;
            return TryGetAttributeBool(nameof(DependencyFolderPath), out flag) && flag;
        }
    }

    public bool IsFarReference
    {
        get
        {
            bool flag;
            return TryGetAttributeBool("FarReference", out flag) && flag;
        }
    }

    public override IItem DefaultValue
    {
        get
        {
            if (defaultValue == null)
            {
                defaultValue = ConstructValue(null);
            }

            return defaultValue;
        }
    }

    public List<object> DropDownList { get; set; }

    public List<object> DynamicDropDownList { get; set; }

    public List<object> StaticDropDownList { get; set; }


    public List<string> AutoSetPropertyNameList { get; set; }

    public bool ParentAttributeResolved { get; set; }

    public IEnumerable<BoollinkStruct> Boollink
    {
        get
        {
            if (boolLinkList_ == null)
            {
                if (!ExistAttribute(nameof(Boollink)))
                {
                    return null;
                }

                boolLinkList_ = new List<BoollinkStruct>();
                var attribute = GetAttribute(nameof(Boollink));
                var chArray = new char[1] {','};
                foreach (var str1 in attribute.Split(chArray))
                {
                    if (!string.IsNullOrEmpty(str1))
                    {
                        var str2 = str1;
                        var flag = false;
                        if (str2.StartsWith("!"))
                        {
                            flag = true;
                            str2 = str2.Replace("!", string.Empty);
                        }

                        var strArray = str2.Split('.');
                        var boollinkStruct = new BoollinkStruct();
                        boollinkStruct.BaseReferenceNum = strArray.Where(x => x.ToLower() == "base").Count();
                        boollinkStruct.PropertyName = strArray[strArray.Count() - 1];
                        boollinkStruct.IsReverse = flag;
                        boolLinkList_.Add(boollinkStruct);
                    }
                }
            }

            return boolLinkList_;
        }
    }

    public IEnumerable<EnumlinkStruct> Enumlink
    {
        get
        {
            if (enumLinkList_ == null)
            {
                if (!ExistAttribute(nameof(Enumlink)))
                {
                    return null;
                }

                enumLinkList_ = new List<EnumlinkStruct>();
                var attribute = GetAttribute(nameof(Enumlink));
                var chArray1 = new char[1] {','};
                foreach (var str in attribute.Split(chArray1))
                {
                    var chArray2 = new char[1] {'/'};
                    var strArray1 = str.Split(chArray2);
                    if (strArray1 != null || strArray1.Length >= 2)
                    {
                        var strArray2 = strArray1[0].Split('.');
                        var enumlinkStruct1 = new EnumlinkStruct();
                        enumlinkStruct1.BaseReferenceNum = strArray2.Where(x => x.ToLower() == "base").Count();
                        enumlinkStruct1.PropertyName = strArray2[strArray2.Count() - 1];
                        enumlinkStruct1.Items = new List<string>();
                        enumlinkStruct1.IsReverse = false;
                        var enumlinkStruct2 = enumlinkStruct1;
                        for (var index = 1; index < strArray1.Length; ++index)
                        {
                            enumlinkStruct2.Items.Add(strArray1[index]);
                        }

                        enumLinkList_.Add(enumlinkStruct2);
                    }
                }
            }

            return enumLinkList_;
        }
    }

    public IEnumerable<EnumlinkStruct> EnumReadOnlyLink
    {
        get
        {
            if (enumReadOnlyLinkList_ == null)
            {
                if (!ExistAttribute(nameof(EnumReadOnlyLink)))
                {
                    return null;
                }

                enumReadOnlyLinkList_ = new List<EnumlinkStruct>();
                var attribute = GetAttribute(nameof(EnumReadOnlyLink));
                var chArray1 = new char[1] {','};
                foreach (var str1 in attribute.Split(chArray1))
                {
                    var chArray2 = new char[1] {'/'};
                    var strArray1 = str1.Split(chArray2);
                    if (strArray1 != null || strArray1.Length >= 2)
                    {
                        var str2 = strArray1[0];
                        var flag = false;
                        if (str2.StartsWith("!"))
                        {
                            flag = true;
                            str2 = str2.Replace("!", string.Empty);
                        }

                        var strArray2 = str2.Split('.');
                        var enumlinkStruct1 = new EnumlinkStruct();
                        enumlinkStruct1.BaseReferenceNum = strArray2.Where(x => x.ToLower() == "base").Count();
                        enumlinkStruct1.PropertyName = strArray2[strArray2.Count() - 1];
                        enumlinkStruct1.Items = new List<string>();
                        enumlinkStruct1.IsReverse = flag;
                        var enumlinkStruct2 = enumlinkStruct1;
                        for (var index = 1; index < strArray1.Length; ++index)
                        {
                            enumlinkStruct2.Items.Add(strArray1[index]);
                        }

                        enumReadOnlyLinkList_.Add(enumlinkStruct2);
                    }
                }
            }

            return enumReadOnlyLinkList_;
        }
    }

    public IntlinkStruct Intlink
    {
        get
        {
            if (intlinkData_ == null)
            {
                if (!ExistAttribute(nameof(Intlink)))
                {
                    return null;
                }

                var strArray1 = GetAttribute(nameof(Intlink)).Split('/');
                if (strArray1.Length < 3)
                {
                    return null;
                }

                var strArray2 = strArray1[0].Split('.');
                var intlinkStruct = new IntlinkStruct();
                intlinkStruct.BaseReferenceNum = strArray2.Where(x => x.ToLower() == "base").Count();
                intlinkStruct.PropertyName = strArray2[strArray2.Count() - 1];
                intlinkStruct.Operator = strArray1[1];
                intlinkStruct.Value = int.Parse(strArray1[2]);
                intlinkData_ = intlinkStruct;
            }

            return intlinkData_;
        }
    }

    public string ReadOnlylink
    {
        get
        {
            if (readOnlylinkData_ == null)
            {
                readOnlylinkData_ = GetAttribute(nameof(ReadOnlylink));
            }

            return readOnlylinkData_;
        }
    }

    public IEnumerable<ObjectApplicationKey> ObjectKeyBrowsable
    {
        get
        {
            if (objectKeyBrowsableList_ == null)
            {
                if (!ExistAttribute(nameof(ObjectKeyBrowsable)))
                {
                    return null;
                }

                objectKeyBrowsableList_ = new List<ObjectApplicationKey>();
                var attribute = GetAttribute(nameof(ObjectKeyBrowsable));
                var chArray1 = new char[1] {';'};
                foreach (var str in attribute.Split(chArray1))
                {
                    var chArray2 = new char[1] {'/'};
                    var strArray = str.Split(chArray2);
                    if (strArray.Count() >= 2)
                    {
                        var result = false;
                        bool.TryParse(strArray[1], out result);
                        objectKeyBrowsableList_.Add(new ObjectApplicationKey
                        {
                            KeyName = strArray[0],
                            IsBrowsable = result
                        });
                    }
                }
            }

            return objectKeyBrowsableList_;
        }
    }

    public IEnumerable<ObjectApplicationKey> ObjectKeyAutoExpandProperty
    {
        get
        {
            if (objectKeyAutoExpandPropertyList_ == null)
            {
                if (!ExistAttribute(nameof(ObjectKeyAutoExpandProperty)))
                {
                    return null;
                }

                objectKeyAutoExpandPropertyList_ = new List<ObjectApplicationKey>();
                var attribute = GetAttribute(nameof(ObjectKeyAutoExpandProperty));
                var chArray1 = new char[1] {';'};
                foreach (var str in attribute.Split(chArray1))
                {
                    var chArray2 = new char[1] {'/'};
                    var strArray = str.Split(chArray2);
                    if (strArray.Count() >= 2)
                    {
                        var result = false;
                        bool.TryParse(strArray[1], out result);
                        objectKeyAutoExpandPropertyList_.Add(new ObjectApplicationKey
                        {
                            KeyName = strArray[0],
                            IsBrowsable = result
                        });
                    }
                }
            }

            return objectKeyAutoExpandPropertyList_;
        }
    }

    public override void matchTypes(ModuleContainer moduleContainer)
    {
        base.matchTypes(moduleContainer);
        DataType = moduleContainer[TypeName];
    }

    public IItem ConstructValue(DataItem parent)
    {
        return ConstructValueFromString(parent, TypeName);
    }

    public IItem ConstructValueFromString(DataItem parent, string constructTypeName)
    {
        var defaultValue = DefaultValueString;
        if (parent != null && parent.DataType is Class)
        {
            var defaultValueOverride = ((Class)parent.DataType).GetDefaultValueOverride(Name);
            if (defaultValueOverride != null)
            {
                defaultValue = defaultValueOverride;
            }
        }

        return DocumentInterface.ModuleContainer.CreateObjectFromString(parent, DataType, constructTypeName,
            defaultValue, true, Pointer);
    }

    public class LinkBase
    {
        public int BaseReferenceNum { get; set; }

        public string PropertyName { get; set; }

        public DataItem GetTargetParentDataItem(DataItem dataItem)
        {
            for (var index = 0; index < BaseReferenceNum; ++index)
            {
                dataItem = dataItem.Parent;
            }

            return dataItem;
        }
    }

    public class BoollinkStruct : LinkBase
    {
        public bool IsReverse { get; set; }

        public bool IsMatch(DataItem dataItem)
        {
            if (dataItem == null)
            {
                return false;
            }

            var targetParentDataItem = GetTargetParentDataItem(dataItem);
            if (targetParentDataItem == null)
            {
                return false;
            }

            var flag = targetParentDataItem.GetBool(PropertyName);
            if (IsReverse)
            {
                flag = !flag;
            }

            return flag;
        }
    }

    public class EnumlinkStruct : LinkBase
    {
        public bool IsReverse { get; set; }

        public List<string> Items { get; set; }

        public bool IsMatch(DataItem dataItem)
        {
            if (dataItem == null)
            {
                return false;
            }

            var targetParentDataItem = GetTargetParentDataItem(dataItem);
            if (targetParentDataItem == null)
            {
                return false;
            }

            var str = targetParentDataItem.GetEnum(PropertyName);
            if (str == null)
            {
                return false;
            }

            var flag = Items.Contains(str);
            return !IsReverse ? flag : !flag;
        }
    }

    public class IntlinkStruct : LinkBase
    {
        public string Operator { get; set; }

        public int Value { get; set; }

        public bool IsMatch(DataItem dataItem)
        {
            if (dataItem == null)
            {
                return false;
            }

            var targetParentDataItem = GetTargetParentDataItem(dataItem);
            if (targetParentDataItem == null)
            {
                return false;
            }

            var obj = targetParentDataItem.GetValue(PropertyName);
            if (obj == null)
            {
                return false;
            }

            var num = obj.GetInt();
            switch (Operator)
            {
                case "==":
                    return num == Value;
                case ">":
                    return num > Value;
                case "<":
                    return num < Value;
                case ">=":
                    return num >= Value;
                case "<=":
                    return num <= Value;
                default:
                    return false;
            }
        }
    }

    public class ObjectApplicationKey
    {
        public string KeyName { get; set; }

        public bool IsBrowsable { get; set; }

        public void SetBrowsable(DataItem targetDataItem, ref bool isBrowsableFlag)
        {
            if (targetDataItem == null)
            {
                return;
            }

            var parentTimeLineTrack = targetDataItem.ParentTimeLineTrack;
            var dataItem = parentTimeLineTrack == null ? targetDataItem.ParentObject :
                !(parentTimeLineTrack["nodes_"] is DynamicArray dynamicArray) || dynamicArray.Count <= 0 ? null :
                dynamicArray[0];
            if (dataItem == null || dataItem.DataType == null)
            {
                return;
            }

            var attribute = dataItem.DataType.GetAttribute(nameof(ObjectApplicationKey));
            if (attribute == null)
            {
                return;
            }

            if (!attribute.Split(',').Contains(KeyName))
            {
                return;
            }

            isBrowsableFlag &= IsBrowsable;
        }

        public AutoExpandPropertyState GetAutoExpandState(DataItem targetDataItem)
        {
            if (targetDataItem == null)
            {
                return AutoExpandPropertyState.AEPS_UNKNOWN;
            }

            var parentObject = targetDataItem.ParentObject;
            if (parentObject != null && parentObject.DataType != null)
            {
                var attribute = parentObject.DataType.GetAttribute(nameof(ObjectApplicationKey));
                if (attribute != null)
                {
                    if (attribute.Split(',').Contains(KeyName))
                    {
                        return IsBrowsable
                            ? AutoExpandPropertyState.AEPS_EXPAND
                            : AutoExpandPropertyState.AEPS_COLLAPSE;
                    }
                }
            }

            return AutoExpandPropertyState.AEPS_UNKNOWN;
        }
    }
}