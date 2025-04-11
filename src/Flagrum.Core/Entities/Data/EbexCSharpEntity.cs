using System.Linq;
using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class CSharpEntity : Entity
{
    private const string AttributeEntity = "LmEntityAttribute";
    private const string AttributeProperty = "LmPropertyAttribute";
    private const string AttributePropertyVariableIn = "LmPropertyVariableInAttribute";
    public const string DefaultEntityName = "SQEX.Ebony.Framework.Entity.TransformEntity";
    public const string CSharpEntityName = "SQEX.Luminous.GameFramework.GameObject.CSharpExtensionEntity";
    private string nativeClassName_ = "SQEX.Ebony.Framework.Entity.TransformEntity";

    public CSharpEntity(DataItem parent, DataType dataType)
        : base(parent, dataType) { }

    public override string AlternativeName => "SQEX.Luminous.GameFramework.GameObject.CSharpExtensionEntity";

    public static bool IsCSharpEntity(DataType dataType)
    {
        var flag = false;
        return dataType.TryGetAttributeBool("LmEntityAttribute", out flag) && flag;
    }

    public static bool IsCSharpField(Field field)
    {
        var flag = false;
        return field.TryGetAttributeBool("LmPropertyAttribute", out flag) && flag;
    }

    public static bool IsCSharpFieldPinDefault(Field field)
    {
        var flag = false;
        return field.TryGetAttributeBool("LmPropertyVariableInAttribute", out flag) && flag;
    }

    protected override void setupDefaultValue(DataType dataType)
    {
        base.setupDefaultValue(dataType);
        if (!(dataType is Class class1))
        {
            return;
        }

        foreach (var field in class1.Fields)
        {
            if (!field.Deprecated)
            {
                var flag = false;
                if (!IsCSharpField(field))
                {
                    if (IsCSharpFieldPinDefault(field))
                    {
                        flag = true;
                    }
                    else
                    {
                        continue;
                    }
                }

                var dataItem = this[field];
                if (dataItem != null)
                {
                    dataItem.IsDynamic = true;
                    if (flag)
                    {
                        dataItem.IsPropertyPin = true;
                    }
                }
            }
        }

        var baseTypes = class1.BaseTypes;
        var class2 = class1.BaseTypes.Where(klass => !IsCSharpEntity(klass)).First();
        if (class2 == null)
        {
            return;
        }

        nativeClassName_ = class2.FullName;
    }
}