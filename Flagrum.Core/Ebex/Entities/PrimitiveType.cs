namespace Flagrum.Core.Ebex.Entities;

public enum PrimitiveType
{
    None,
    Bool,
    String,
    Char,
    UChar,
    Short,
    UShort,
    Int,
    UInt,
    Long,
    ULong,
    Float,
    Double,
    Float4,
    Fixid,
    Color,
    Pointer,
    Enum,
    ObjectReference
}

public class PrimitiveTypeHelper
{
    public static PrimitiveType FromCompositeString(string name)
    {
        var index = name.LastIndexOf('.');
        if (index >= 0)
        {
            name = name[(index + 1)..];
        }

        return FromString(name);
    }

    public static PrimitiveType FromString(string name)
    {
        switch (name)
        {
            case "BaseObjectReference":
                return PrimitiveType.ObjectReference;
            case "Color":
                return PrimitiveType.Color;
            case "Fixid":
                return PrimitiveType.Fixid;
            case "LmVector4":
            case "Vector":
            case "VectorA":
            case "float4":
                return PrimitiveType.Float4;
            case "String":
            case "string":
                return PrimitiveType.String;
            case "bool":
                return PrimitiveType.Bool;
            case "char":
            case "int8_t":
            case "signed char":
                return PrimitiveType.Char;
            case "double":
                return PrimitiveType.Double;
            case "enum":
                return PrimitiveType.Enum;
            case "float":
                return PrimitiveType.Float;
            case "int":
            case "int32_t":
            case "signed int":
                return PrimitiveType.Int;
            case "int16_t":
            case "short":
            case "signed short":
                return PrimitiveType.Short;
            case "int64_t":
            case "long long":
            case "signed long long":
                return PrimitiveType.Long;
            case "pointer":
                return PrimitiveType.Pointer;
            case "size_t":
            case "uint":
            case "uint32_t":
            case "unsigned int":
                return PrimitiveType.UInt;
            case "uint16_t":
            case "unsigned short":
                return PrimitiveType.UShort;
            case "uint64_t":
            case "unsigned long long":
                return PrimitiveType.ULong;
            case "uint8_t":
            case "unsigned char":
                return PrimitiveType.UChar;
            default:
                return PrimitiveType.None;
        }
    }
}