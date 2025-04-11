using System.Linq;
using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class SequenceCSharpNodeDataItem : SequenceActivatableNodeDataItem
{
    public new const string ClassFullName = "SQEX.Luminous.GameFramework.Sequence.SequenceCSharpNode";
    private const string PropertyNameNamespaceName = "namespaceName_";
    private const string PropertyNameClassName = "className_";

    public static readonly Class SequenceCSharpNodeClass =
        DocumentInterface.ModuleContainer["SQEX.Luminous.GameFramework.Sequence.SequenceCSharpNode"] as Class;

    public SequenceCSharpNodeDataItem(DataItem parent, DataType dataType)
        : base(parent, dataType)
    {
        var strArray = dataType.FullName.Split('.');
        var str = "";
        for (var index = 0; index < strArray.Length - 1; ++index)
        {
            str = str + strArray[index] + ".";
        }

        SetString("namespaceName_", str.TrimEnd('.'));
        SetString("className_", strArray.Last());
    }

    public override string AlternativeName => "SQEX.Luminous.GameFramework.Sequence.SequenceCSharpNode";

    protected override void setupDefaultValue(DataType dataType)
    {
        if (!(dataType is Class @class))
        {
            return;
        }

        foreach (var field in @class.Fields)
        {
            if (!field.Deprecated)
            {
                var flag1 = false;
                var flag2 = false;
                if (!field.TryGetAttributeBool("LmPropertyAttribute", out flag1))
                {
                    if (field.TryGetAttributeBool("LmPropertyVariableInAttribute", out flag1))
                    {
                        flag2 = true;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (flag1)
                {
                    DataItem dataItem = null;
                    string constructTypeName = null;
                    if (field.TryGetAttributeString("PinValueType", out constructTypeName))
                    {
                        dataItem = new FieldDataItem(this, field);
                        dataItem.Value = field.ConstructValueFromString(this, constructTypeName);
                    }
                    else
                    {
                        if (field.DataType != null)
                        {
                            dataItem = DocumentInterface.ModuleContainer.CreateObjectFromString(this,
                                field.DataType) as DataItem;
                        }

                        if (dataItem == null)
                        {
                            dataItem = new FieldDataItem(this, field);
                            dataItem.Value = field.ConstructValue(this);
                        }
                    }

                    dataItem.Name = field.Name;
                    dataItem.Field = field;
                    dataItem.IsDynamic = true;
                    if (flag2)
                    {
                        dataItem.IsPropertyPin = true;
                    }
                }
            }
        }

        foreach (DataType baseType in @class.BaseTypes)
        {
            base.setupDefaultValue(baseType);
        }
    }
}