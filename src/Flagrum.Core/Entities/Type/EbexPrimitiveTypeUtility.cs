using System;
using System.Collections.Generic;
using Flagrum.Core.Scripting.Ebex.Data;

namespace Flagrum.Core.Scripting.Ebex.Type;

public class PrimitiveTypeUtility
{
    public static PrimitiveType FromNameSimple(string typeName)
    {
        return typeName != null ? FromNameImpl(typeName) : PrimitiveType.None;
    }

    public static PrimitiveType FromName(string typeName)
    {
        if (typeName == null)
        {
            return PrimitiveType.None;
        }

        var shortName = typeName;
        var num = shortName.LastIndexOf('.');
        if (num >= 0)
        {
            shortName = shortName.Substring(num + 1);
        }

        return FromNameImpl(shortName);
    }

    private static PrimitiveType FromNameImpl(string shortName)
    {
        switch (shortName)
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

    public static string ToName(PrimitiveType type)
    {
        switch (type)
        {
            case PrimitiveType.None:
                return null;
            case PrimitiveType.Bool:
                return "bool";
            case PrimitiveType.String:
                return "string";
            case PrimitiveType.Char:
                return "char";
            case PrimitiveType.UChar:
                return "unsigned char";
            case PrimitiveType.Short:
                return "short";
            case PrimitiveType.UShort:
                return "unsigned short";
            case PrimitiveType.Int:
                return "int";
            case PrimitiveType.UInt:
                return "unsigned int";
            case PrimitiveType.Long:
                return "int64_t";
            case PrimitiveType.ULong:
                return "uint64_t";
            case PrimitiveType.Float:
                return "float";
            case PrimitiveType.Double:
                return "double";
            case PrimitiveType.Float4:
                return "float4";
            case PrimitiveType.Fixid:
                return "Fixid";
            case PrimitiveType.Color:
                return "Color";
            case PrimitiveType.Pointer:
                return "pointer";
            case PrimitiveType.Enum:
                return "enum";
            default:
                throw new ArgumentException();
        }
    }

    public static System.Type ToSystemType(PrimitiveType type)
    {
        switch (type)
        {
            case PrimitiveType.None:
                return null;
            case PrimitiveType.Bool:
                return typeof(bool);
            case PrimitiveType.String:
                return typeof(string);
            case PrimitiveType.Char:
                return typeof(sbyte);
            case PrimitiveType.UChar:
                return typeof(byte);
            case PrimitiveType.Short:
                return typeof(short);
            case PrimitiveType.UShort:
                return typeof(ushort);
            case PrimitiveType.Int:
                return typeof(int);
            case PrimitiveType.UInt:
                return typeof(uint);
            case PrimitiveType.Long:
                return typeof(long);
            case PrimitiveType.ULong:
                return typeof(ulong);
            case PrimitiveType.Float:
                return typeof(float);
            case PrimitiveType.Double:
                return typeof(double);
            case PrimitiveType.Float4:
                return typeof(float[]);
            case PrimitiveType.Fixid:
                return typeof(string);
            case PrimitiveType.Color:
                return typeof(Color);
            case PrimitiveType.Pointer:
                return typeof(object);
            case PrimitiveType.Enum:
                return typeof(string);
            default:
                throw new ArgumentException();
        }
    }

    public static System.Type ToSystemListType(PrimitiveType type)
    {
        var systemType = ToSystemType(type);
        if (systemType == typeof(bool))
        {
            return typeof(List<bool>);
        }

        if (systemType == typeof(sbyte))
        {
            return typeof(List<sbyte>);
        }

        if (systemType == typeof(byte))
        {
            return typeof(List<byte>);
        }

        if (systemType == typeof(short))
        {
            return typeof(List<short>);
        }

        return systemType == typeof(ushort) ? typeof(List<ushort>) : null;
    }
}