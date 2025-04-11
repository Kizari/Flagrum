using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class Object : DataItem
{
    public const string ClassFullName = "SQEX.Luminous.Core.Object.Object";

    public Object(DataItem parent, string typeFullName)
        : base(parent, DocumentInterface.ModuleContainer[typeFullName]) { }

    public Object(DataItem parent, DataType dataType)
        : base(parent, dataType) { }

    public int ObjectIndex { get; set; }
    
    protected DataItem createEditorOnlyItemInner(string keyName, string displayName = null)
    {
        if (DocumentInterface.ModuleContainer == null)
        {
            return null;
        }

        if (this[keyName] != null)
        {
            Remove(this[keyName]);
        }

        var field = new Field(keyName, PrimitiveTypeUtility.ToName(PrimitiveType.String), false);
        if (displayName != null)
        {
            field.DisplayName = displayName;
        }

        field.Attributes["EditorOnly"] = "True";
        var valueDataItem = new ValueDataItem(this, field);
        valueDataItem.Name = keyName;
        valueDataItem.Field = field;
        valueDataItem.Value = new Value(string.Empty);
        valueDataItem.Modified = false;
        return valueDataItem;
    }

    protected DynamicArray createEditorOnlyDynamicArrayInner(
        string keyName,
        string typeName)
    {
        if (DocumentInterface.ModuleContainer == null)
        {
            return null;
        }

        if (this[keyName] != null)
        {
            Remove(this[keyName]);
        }

        var field = new Field(keyName, string.Format("SQEX.Ebony.Std.DynamicArray<{0}>", typeName), false);
        field.Attributes["EditorOnly"] = "True";
        var dynamicArray = new DynamicArray(this);
        dynamicArray.Name = keyName;
        dynamicArray.Field = field;
        dynamicArray.Modified = false;
        dynamicArray.Field.Attributes["Browsable"] = "False";
        dynamicArray.Browsable = false;
        return dynamicArray;
    }

    protected void createCommentItem()
    {
        // var editorOnlyItemInner = createEditorOnlyItemInner("comment_", TextResourcesJenovaData.ObjectComment);
        // if (editorOnlyItemInner == null)
        // {
        //     return;
        // }
        //
        // editorOnlyItemInner.Field.Attributes["Category"] = TextResourcesJenovaData.ObjectCategoryCommon;
        // editorOnlyItemInner.Field.Attributes["MultiLine"] = "3";
        //throw new NotImplementedException();
    }

    protected void createGroupArray()
    {
        var dynamicArrayInner =
            createEditorOnlyDynamicArrayInner("groups_", "SQEX.Ebony.Framework.Sequence.Group.SequenceGroupBase*");
        if (dynamicArrayInner == null)
        {
            return;
        }

        dynamicArrayInner.Field.Attributes["AsBaseContainer"] = string.Empty;
    }

    protected void createConnectorArray()
    {
        var dynamicArrayInner = createEditorOnlyDynamicArrayInner("connectors_",
            "SQEX.Ebony.Framework.Sequence.Connector.SequenceConnectorBase*");
        if (dynamicArrayInner == null)
        {
            return;
        }

        dynamicArrayInner.Field.Attributes["AsBaseContainer"] = string.Empty;
    }

    public static bool IsNotBasedOnObjectStatic(Class c)
    {
        return c != null &&
               !c.IsBasedOn(DocumentInterface.ModuleContainer["SQEX.Luminous.Core.Object.Object"] as Class);
    }
}