using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class DynamicArray : DataItem
{
    public const string ClassFullName = "SQEX.Ebony.Std.DynamicArray";
    public const string CompatibleClassFullName = "SQEX.Ebony.Std.IntrusivePointerDynamicArray";

    public DynamicArray(DataItem parent)
        : base(parent, DocumentInterface.ModuleContainer["SQEX.Ebony.Std.DynamicArray"]) { }

    public int Count => Children.Count;

    public DataItem this[int index]
    {
        get => Children[index];
        set => Children[index] = value;
    }

    public void CloneArrayTo(DynamicArray targetArray)
    {
        targetArray.ClearChild();
        foreach (var child in Children)
        {
            DataItem dataItem = null;
            if (child is DynamicArray)
            {
                dataItem = new DynamicArray(targetArray);
                ((DynamicArray)child).CloneArrayTo((DynamicArray)dataItem);
            }
            else if (child.DataType != null)
            {
                dataItem = child.Clone(targetArray);
            }
            else if (child.Value is Value)
            {
                var name = PrimitiveTypeUtility.ToName(((Value)child.Value).PrimitiveType);
                dataItem = new ValueDataItem(targetArray, name);
                dataItem.Value = child.Value;
            }

            if (dataItem != null)
            {
                dataItem.name = child.name;
                dataItem.Field = child.Field;
                dataItem.IsDynamic = child.IsDynamic;
            }
        }
    }

    public void AutoCreateItem()
    {
        var attribute = Field.GetAttribute("DynamicArrayAutoCreateElement");
        if (attribute == null)
        {
            return;
        }

        var result = 0;
        if (!int.TryParse(attribute, out result))
        {
            return;
        }

        for (var count = Children.Count; count < result; ++count)
        {
            AddDynamicArrayItemProperty(this);
        }
    }

    public static DataItem AddDynamicArrayItemProperty(
        DynamicArray dynamicArray,
        Class overrideClass = null)
    {
        if (dynamicArray.Field != null)
        {
            var num1 = 0;
            if (dynamicArray.Field.TryGetAttributeInt("ArrayNumMax", out num1) &&
                dynamicArray.Children.Count == num1)
            {
                // var num2 = (int)DocumentInterface.DocumentContainer.ShowMessageBox("リストの要素数が指定の最大値（" + num1 +
                //     "）を超えています");
                return null;
            }
        }

        if (dynamicArray.ParentPackage is Prefab parentPackage)
        {
            if (Prefab.DifferenceMode)
            {
                // var num = (int)DocumentInterface.DocumentContainer.ShowMessageBox(
                //     "差分編集モードでは、DynamicArrayの要素を追加できません\n（北出までご相談下さい）");
                return null;
            }

            if (Prefab.CharaEntryPrefabMode && parentPackage["differences"] != null)
            {
                // var num = (int)DocumentInterface.DocumentContainer.ShowMessageBox(
                //     "キャラエントリモードで、差分のあるPrefabに、DynamicArrayの要素を追加できません\n（北出までご相談下さい）");
                return null;
            }
        }

        var empty = string.Empty;
        string str1;
        Class c;
        if (overrideClass != null)
        {
            str1 = overrideClass.FullName;
            c = overrideClass;
        }
        else
        {
            str1 = dynamicArray.Field.TemplateTypeNames[0];
            if (str1.EndsWith("*"))
            {
                str1 = str1.Substring(0, str1.Length - 1);
            }

            c = DocumentInterface.ModuleContainer[str1] as Class;
        }

        var attribute = dynamicArray.Field.GetAttribute("DynamicArrayItemNamePrefix");
        var childPropertyName = createChildPropertyName(dynamicArray, attribute);
        var type = PrimitiveTypeUtility.FromName(str1);

        Enum @enum = null;
        if (DocumentInterface.ModuleContainer[str1] is Enum en)
        {
            type = PrimitiveType.Enum;
            @enum = en;
        }

        DataItem parentItem3 = null;

        if (type == PrimitiveType.None && Object.IsNotBasedOnObjectStatic(c))
        {
            if (DocumentInterface.ModuleContainer.CreateObjectFromString(dynamicArray, c) is DataItem parentItem2)
            {
                parentItem2.AutoCreateDynamicArrayItem();
                ModuleContainer.ApplyUseParentAttribute(parentItem2);
                parentItem2.Name = childPropertyName;
                parentItem2.Field = new Field(childPropertyName, str1, false);
                parentItem2.IsDynamic = true;
                parentItem3 = parentItem2;
            }
        }
        else
        {
            var str2 = PrimitiveTypeUtility.ToName(type);
            if (@enum != null)
            {
                str2 = @enum.FullName;
            }

            var childPropertyIndex = getChildPropertyIndex(dynamicArray, attribute);
            var field = new Field(childPropertyName, str2, false);
            field.DisplayName = "[" + childPropertyIndex + "]";
            if (type == PrimitiveType.Enum)
            {
                field.matchTypes(DocumentInterface.ModuleContainer);
            }

            var valueDataItem = new ValueDataItem(dynamicArray, str2);
            valueDataItem.Name = childPropertyName;
            valueDataItem.Field = field;
            valueDataItem.Value =
                new Value(type, type == PrimitiveType.Enum ? @enum.DefaultEnumItem.FullName : null);
            parentItem3 = valueDataItem;
            parentItem3.IsDynamic = true;
        }

        dynamicArray.CheckDifference();
        return parentItem3;
    }

    public static void RenameChildrenSequence(DynamicArray dynamicArray)
    {
        if (dynamicArray.Children.Count == 0)
        {
            return;
        }

        var num = 0;
        var str = dynamicArray.Field.GetAttribute("DynamicArrayItemNamePrefix") ?? dynamicArray.Name;
        foreach (var child in dynamicArray.Children)
        {
            child.Name = str + num++;
            child.Field = new Field(child.Name, child.Field.TypeName, false);
        }
    }

    public static DataItem Replace(
        DynamicArray dynamicArray,
        DataItem dataItem,
        Class dataClass)
    {
        if (!(dataItem.DataType is Class dataType))
        {
            return null;
        }

        if (dataType == dataClass)
        {
            return null;
        }

        if (Object.IsNotBasedOnObjectStatic(dataClass))
        {
            var name = dataItem.Name;
            var index = dynamicArray.Children.IndexOf(dataItem);
            dynamicArray.Remove(dataItem);
            if (DocumentInterface.ModuleContainer.CreateObjectFromString(dynamicArray, dataClass) is DataItem
                objectFromString2)
            {
                objectFromString2.Name = name;
                objectFromString2.Field = new Field(objectFromString2.Name, dataClass.FullName, false);
                objectFromString2.IsDynamic = true;
                dynamicArray.Children.Remove(objectFromString2);
                dynamicArray.Children.Insert(index, objectFromString2);
                objectFromString2.AutoCreateDynamicArrayItem();
                return objectFromString2;
            }
        }

        return null;
    }

    public static string createChildPropertyName(
        DynamicArray parentArray,
        string inPrefixName = null,
        string inSuffixName = null)
    {
        var str1 = inPrefixName ?? parentArray.Name;
        var str2 = inSuffixName ?? string.Empty;
        var num = 0;
        var flag = false;
        while (!flag)
        {
            flag = true;
            foreach (var parent in parentArray)
            {
                if (parent.Name == str1 + num + str2)
                {
                    flag = false;
                    ++num;
                    break;
                }
            }
        }

        return str1 + num + str2;
    }

    public static int getChildPropertyIndex(
        DynamicArray parentArray,
        string inPrefixName = null,
        string inSuffixName = null)
    {
        var str1 = inPrefixName ?? parentArray.Name;
        var str2 = inSuffixName ?? string.Empty;
        var num = 0;
        var flag = false;
        while (!flag)
        {
            flag = true;
            foreach (var parent in parentArray)
            {
                if (parent.Name == str1 + num + str2)
                {
                    flag = false;
                    ++num;
                    break;
                }
            }
        }

        return num;
    }
}