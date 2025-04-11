using System.Collections.Generic;
using System.Linq;
using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class GraphPinDataItem : DataItem
{
    public const string ClassFullName = "SQEX.Ebony.Framework.Node.GraphPin";

    public GraphPinDataItem(DataItem parent, DataType dataType)
        : base(parent, dataType) { }

    public string Guid { get; set; } = System.Guid.NewGuid().ToString();

    public DynamicArray Connections => this["connections_"] as DynamicArray;

    public string PinName
    {
        get
        {
            var pinName = GetString("pinName_");
            return string.IsNullOrEmpty(pinName) ? DisplayName : pinName;
        }
    }

    public PinValueType PinType
    {
        get
        {
            var pinValueType = PinValueType;
            return pinValueType == null ? _typeMap["0"] : _typeMap[pinValueType];
        }
    }
    
    public string PinValueType
    {
        get
        {
            if (field != null && field.PinValueType != null)
            {
                return field.PinValueType;
            }

            var dataType = this.dataType;
            return null;
        }
    }

    public int MaxConnection
    {
        get
        {
            if (field != null)
            {
                return field.MaxConnection;
            }

            var dataType = this.dataType;
            return -1;
        }
    }

    public bool DynamicPin
    {
        get
        {
            var specialType = SpecialType;
            return specialType == "DynamicTriggerInputPin" || specialType == "DynamicTriggerOutputPin" ||
                   specialType == "DynamicVariableInputPin" || specialType == "DynamicVariableOutputPin";
        }
    }

    private static readonly Dictionary<string, PinValueType> _typeMap = new()
    {
        {"Object", new PinValueType(System.Drawing.Color.BlueViolet, PrimitiveType.ObjectReference)},
        {"Vector", new PinValueType(System.Drawing.Color.GreenYellow, PrimitiveType.Float4)},
        {"Float", new PinValueType(System.Drawing.Color.Turquoise, PrimitiveType.Float)},
        {"Int", new PinValueType(System.Drawing.Color.CornflowerBlue, PrimitiveType.Int)},
        {"Pointer", new PinValueType(System.Drawing.Color.Firebrick, PrimitiveType.Pointer)},
        {"Bool", new PinValueType(System.Drawing.Color.LightCoral, PrimitiveType.Bool)},
        {"Actors", new PinValueType(System.Drawing.Color.Gold, PrimitiveType.None)},
        {"List", new PinValueType(System.Drawing.Color.Gold, PrimitiveType.None)},
        {"Object/Vector", new PinValueType(System.Drawing.Color.White, PrimitiveType.ObjectReference, PrimitiveType.Float4)},
        {"Fixid", new PinValueType(System.Drawing.Color.OrangeRed, PrimitiveType.Fixid)},
        {"float", new PinValueType(System.Drawing.Color.Turquoise, PrimitiveType.Float)},
        {"FixID", new PinValueType(System.Drawing.Color.OrangeRed, PrimitiveType.Fixid)},
        {"Object/List", new PinValueType(System.Drawing.Color.White, PrimitiveType.None, PrimitiveType.ObjectReference)},
        {"Actor", new PinValueType(System.Drawing.Color.BlueViolet, PrimitiveType.ObjectReference)},
        {"Foat", new PinValueType(System.Drawing.Color.Turquoise, PrimitiveType.Float)},
        {"Vector/Object", new PinValueType(System.Drawing.Color.White, PrimitiveType.Float4, PrimitiveType.ObjectReference)},
        {"String", new PinValueType(System.Drawing.Color.HotPink, PrimitiveType.String)},
        {"Int/Float/Vector/Bool", new PinValueType(System.Drawing.Color.White, PrimitiveType.Int, PrimitiveType.Float, PrimitiveType.Float4, PrimitiveType.Bool)},
        {"Int/Float", new PinValueType(System.Drawing.Color.White, PrimitiveType.Int, PrimitiveType.Float)},
        {"Fixid/Object/List", new PinValueType(System.Drawing.Color.White, PrimitiveType.Fixid, PrimitiveType.ObjectReference, PrimitiveType.None)},
        {"Int/Fixid", new PinValueType(System.Drawing.Color.White, PrimitiveType.Int, PrimitiveType.Fixid)},
        {"int", new PinValueType(System.Drawing.Color.CornflowerBlue, PrimitiveType.Int)},
        {"bool", new PinValueType(System.Drawing.Color.LightCoral, PrimitiveType.Bool)},
        {"Attacker", new PinValueType(System.Drawing.Color.BlueViolet, PrimitiveType.ObjectReference)},
        {"UInt", new PinValueType(System.Drawing.Color.CornflowerBlue, PrimitiveType.Int)},
        {"Int/Float/Fixid/String", new PinValueType(System.Drawing.Color.White, PrimitiveType.Int, PrimitiveType.Float, PrimitiveType.Fixid, PrimitiveType.String)},
        {"Int/Float/Fixid/String/List", new PinValueType(System.Drawing.Color.White, PrimitiveType.Int, PrimitiveType.Float, PrimitiveType.Fixid, PrimitiveType.String, PrimitiveType.None)},
        {"String/Int/Fixid", new PinValueType(System.Drawing.Color.White, PrimitiveType.String, PrimitiveType.Int, PrimitiveType.Fixid)},
        {"String/Int/List", new PinValueType(System.Drawing.Color.White, PrimitiveType.String, PrimitiveType.Int, PrimitiveType.None)},
        {"FIXID", new PinValueType(System.Drawing.Color.Coral, PrimitiveType.Fixid)},
        {"INT", new PinValueType(System.Drawing.Color.CornflowerBlue, PrimitiveType.Int)},
        {"Float/Int", new PinValueType(System.Drawing.Color.White, PrimitiveType.Float, PrimitiveType.Int)},
        {"Object/Vector/Float", new PinValueType(System.Drawing.Color.White, PrimitiveType.ObjectReference, PrimitiveType.Float4, PrimitiveType.Float)},
        {"Int/Float/Bool", new PinValueType(System.Drawing.Color.White, PrimitiveType.Bool, PrimitiveType.Float, PrimitiveType.Int)},
        {"Fxid", new PinValueType(System.Drawing.Color.OrangeRed, PrimitiveType.Fixid)},
        {"Vector(d)", new PinValueType(System.Drawing.Color.GreenYellow, PrimitiveType.Float4)},
        {"BOOL", new PinValueType(System.Drawing.Color.LightCoral, PrimitiveType.Bool)},
        {"minDistance", new PinValueType(System.Drawing.Color.Turquoise, PrimitiveType.Float)},
        {"0", new PinValueType(System.Drawing.Color.DarkGray, PrimitiveType.None)},
        {"object", new PinValueType(System.Drawing.Color.BlueViolet, PrimitiveType.ObjectReference)},
        {"uint16_t", new PinValueType(System.Drawing.Color.CornflowerBlue, PrimitiveType.Short)},
        {"Uint", new PinValueType(System.Drawing.Color.CornflowerBlue, PrimitiveType.UInt)},
        {"Fixid/UInt", new PinValueType(System.Drawing.Color.White, PrimitiveType.Fixid, PrimitiveType.UInt)},
        {"Value", new PinValueType(System.Drawing.Color.White, System.Enum.GetValues(typeof(PrimitiveType)).Cast<PrimitiveType>().ToArray())},
        {"ObjectReference", new PinValueType(System.Drawing.Color.BlueViolet, PrimitiveType.ObjectReference)}
    };
}

public class PinValueType
{
    public PinValueType(System.Drawing.Color color, params PrimitiveType[] compatibleTypes)
    {
        Color = color;
        CompatibleTypes = compatibleTypes.ToList();
    }
    
    public System.Drawing.Color Color { get; set; }
    public List<PrimitiveType> CompatibleTypes { get; set; }
}