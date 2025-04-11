using System;
using System.IO;
using System.Linq;

namespace Scarlet;

internal static class ExtensionMethods
{
    public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
    {
        if (val.CompareTo(min) < 0)
        {
            return min;
        }

        if (val.CompareTo(max) > 0)
        {
            return max;
        }

        return val;
    }

    /* http://stackoverflow.com/a/23177585 */
    public static bool IsValid<TEnum>(this TEnum enumValue) where TEnum : struct
    {
        var firstChar = enumValue.ToString()[0];
        return (firstChar < '0' || firstChar > '9') && firstChar != '-';
    }

    /* http://stackoverflow.com/a/18375526 */
    public static bool InheritsFrom(this Type type, Type baseType)
    {
        if (type == null)
        {
            return false;
        }

        if (baseType == null)
        {
            return type.IsInterface;
        }

        if (baseType.IsInterface)
        {
            return type.GetInterfaces().Contains(baseType);
        }

        var currentType = type;
        while (currentType != null)
        {
            if (currentType.BaseType == baseType)
            {
                return true;
            }

            currentType = currentType.BaseType;
        }

        return false;
    }

    /* https://stackoverflow.com/a/25577853 */
    public static string ReadNullTerminatedString(this BinaryReader reader)
    {
        var str = "";
        char ch;
        while ((ch = reader.ReadChar()) != 0)
        {
            str = str + ch;
        }

        return str;
    }
}