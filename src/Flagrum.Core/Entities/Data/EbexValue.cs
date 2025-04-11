using System;
using System.Linq;
using Flagrum.Core.Scripting.Ebex.Type;
using Enum = Flagrum.Core.Scripting.Ebex.Type.Enum;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class Value : IItem, IDisposable
{
    public Value()
    {
        PrimitiveType = PrimitiveType.None;
        Object = null;
    }

    public Value(Value v)
    {
        PrimitiveType = v.PrimitiveType;
        Object = v.Object;
    }

    public Value(PrimitiveType type, object obj)
    {
        PrimitiveType = type;
        Object = obj;
    }

    public Value(PrimitiveType type, string text)
    {
        PrimitiveType = type;
        Object = null;
        switch (type)
        {
            case PrimitiveType.Bool:
                var result1 = false;
                bool.TryParse(text, out result1);
                Object = result1;
                break;
            case PrimitiveType.String:
                Object = text;
                break;
            case PrimitiveType.Char:
                sbyte result2 = 0;
                sbyte.TryParse(text, out result2);
                Object = result2;
                break;
            case PrimitiveType.UChar:
                byte result3 = 0;
                byte.TryParse(text, out result3);
                Object = result3;
                break;
            case PrimitiveType.Short:
                short result4 = 0;
                short.TryParse(text, out result4);
                Object = result4;
                break;
            case PrimitiveType.UShort:
                ushort result5 = 0;
                ushort.TryParse(text, out result5);
                Object = result5;
                break;
            case PrimitiveType.Int:
                var result6 = 0;
                int.TryParse(text, out result6);
                Object = result6;
                break;
            case PrimitiveType.UInt:
                uint result7 = 0;
                uint.TryParse(text, out result7);
                Object = result7;
                break;
            case PrimitiveType.Long:
                long result8 = 0;
                long.TryParse(text, out result8);
                Object = result8;
                break;
            case PrimitiveType.ULong:
                ulong result9 = 0;
                ulong.TryParse(text, out result9);
                Object = result9;
                break;
            case PrimitiveType.Float:
                var result10 = 0.0f;
                float.TryParse(text, out result10);
                Object = result10;
                break;
            case PrimitiveType.Double:
                var result11 = 0.0;
                double.TryParse(text, out result11);
                Object = result11;
                break;
            case PrimitiveType.Float4:
                var numArray = new float[4];
                string[] strArray1;
                if (text == null)
                {
                    strArray1 = new string[0];
                }
                else
                {
                    strArray1 = text.Split(',');
                }

                var strArray2 = strArray1;
                int index;
                for (index = 0; index < strArray2.Length && index < 4; ++index)
                {
                    float.TryParse(strArray2[index], out numArray[index]);
                }

                for (; index < 4; ++index)
                {
                    numArray[index] = 0.0f;
                }

                Object = numArray;
                break;
            case PrimitiveType.Fixid:
                Object = text;
                break;
            case PrimitiveType.Color:
                Object = getColorFromString(text);
                break;
            case PrimitiveType.Pointer:
                if (DocumentInterface.DocumentContainer == null)
                {
                    break;
                }

                Object = DocumentInterface.DocumentContainer.FindDataItem(new ItemPath(text));
                break;
            case PrimitiveType.Enum:
                Object = text;
                break;
            default:
                throw new ArgumentException();
        }
    }

    public Value(string value)
    {
        PrimitiveType = PrimitiveType.String;
        Object = value;
    }

    public Value(bool value)
    {
        PrimitiveType = PrimitiveType.Bool;
        Object = value;
    }

    public Value(sbyte value)
    {
        PrimitiveType = PrimitiveType.Char;
        Object = value;
    }

    public Value(byte value)
    {
        PrimitiveType = PrimitiveType.UChar;
        Object = value;
    }

    public Value(short value)
    {
        PrimitiveType = PrimitiveType.Short;
        Object = value;
    }

    public Value(ushort value)
    {
        PrimitiveType = PrimitiveType.UShort;
        Object = value;
    }

    public Value(int value)
    {
        PrimitiveType = PrimitiveType.Int;
        Object = value;
    }

    public Value(uint value)
    {
        PrimitiveType = PrimitiveType.UInt;
        Object = value;
    }

    public Value(long value)
    {
        PrimitiveType = PrimitiveType.Long;
        Object = value;
    }

    public Value(ulong value)
    {
        PrimitiveType = PrimitiveType.ULong;
        Object = value;
    }

    public Value(float value)
    {
        PrimitiveType = PrimitiveType.Float;
        Object = value;
    }

    public Value(double value)
    {
        PrimitiveType = PrimitiveType.Double;
        Object = value;
    }

    public Value(float x, float y, float z, float w)
    {
        PrimitiveType = PrimitiveType.Float4;
        Object = new float[4] {x, y, z, w};
    }

    public Value(Color value)
    {
        PrimitiveType = PrimitiveType.Color;
        Object = value;
    }

    public Value(DataItem dataItem)
    {
        PrimitiveType = PrimitiveType.Pointer;
        Object = dataItem;
    }

    public Value(Enum e, string value)
    {
        if (!e.Contains(value) && e.DefaultEnumItem != null)
        {
            value = e.DefaultEnumItem.Name;
        }

        PrimitiveType = PrimitiveType.Enum;
        Object = value;
    }

    public PrimitiveType PrimitiveType { get; private set; }

    public object Object { get; set; }

    public string TypeName => PrimitiveTypeUtility.ToName(PrimitiveType);

    public void Dispose()
    {
        PrimitiveType = PrimitiveType.None;
        Object = null;
    }

    public System.Type GetSystemType()
    {
        return PrimitiveTypeUtility.ToSystemType(PrimitiveType);
    }

    private object ConvertEnumCommon(Enum typeEnum, object obj, ConvertEnumType type)
    {
        if (PrimitiveType != PrimitiveType.Enum || typeEnum == null || !(obj is string))
        {
            return obj;
        }

        if (type == ConvertEnumType.CET_DISPLAYNAME_2_ENUMNAME)
        {
            var fromDisplayName = typeEnum.FindFromDisplayName(obj as string);
            return fromDisplayName == null ? null : (object)fromDisplayName.Name;
        }

        var enumItem = typeEnum.Find(obj as string);
        return enumItem == null ? null : (object)enumItem.DisplayName;
    }

    public void ConvertEnum_DisplayName2EnumName(Enum e)
    {
        Object = GetEnum_EnumNameObject(e, Object);
    }

    public void ConvertEnum_EnumName2DisplayName(Enum e)
    {
        Object = GetEnum_DisplayNameObject(e, Object);
    }

    public void SetEnum_DisplayName2EnumName(Enum e, object obj)
    {
        Object = GetEnum_EnumNameObject(e, obj);
    }

    public void SetEnum_EnumName2DisplayName(Enum e, object obj)
    {
        Object = GetEnum_DisplayNameObject(e, obj);
    }

    public object GetEnum_EnumNameObject(Enum e, object obj)
    {
        return ConvertEnumCommon(e, obj, ConvertEnumType.CET_DISPLAYNAME_2_ENUMNAME);
    }

    public object GetEnum_DisplayNameObject(Enum e, object obj)
    {
        return ConvertEnumCommon(e, obj, ConvertEnumType.CET_ENUMNAME_2_DISPLAYNAME);
    }

    public bool GetBool()
    {
        if (Object != null)
        {
            if (Object is bool)
            {
                return (bool)Object;
            }

            bool result;
            if (bool.TryParse(Object.ToString(), out result))
            {
                return result;
            }
        }

        return false;
    }

    public sbyte GetChar()
    {
        if (Object is sbyte)
        {
            return (sbyte)Object;
        }

        sbyte result;
        sbyte.TryParse(Object.ToString(), out result);
        return result;
    }

    public byte GetUChar()
    {
        if (Object is byte)
        {
            return (byte)Object;
        }

        byte result;
        byte.TryParse(Object.ToString(), out result);
        return result;
    }

    public short GetShort()
    {
        if (Object is short)
        {
            return (short)Object;
        }

        short result;
        short.TryParse(Object.ToString(), out result);
        return result;
    }

    public ushort GetUShort()
    {
        if (Object is ushort)
        {
            return (ushort)Object;
        }

        ushort result;
        ushort.TryParse(Object.ToString(), out result);
        return result;
    }

    public int GetInt()
    {
        if (Object is int)
        {
            return (int)Object;
        }

        int result;
        int.TryParse(Object.ToString(), out result);
        return result;
    }

    public uint GetUInt()
    {
        if (Object is uint)
        {
            return (uint)Object;
        }

        uint result;
        uint.TryParse(Object.ToString(), out result);
        return result;
    }

    public long GetLong()
    {
        if (Object is long)
        {
            return (long)Object;
        }

        long result;
        long.TryParse(Object.ToString(), out result);
        return result;
    }

    public ulong GetULong()
    {
        if (Object is ulong)
        {
            return (ulong)Object;
        }

        ulong result;
        ulong.TryParse(Object.ToString(), out result);
        return result;
    }

    public float GetFloat()
    {
        if (Object is float)
        {
            return (float)Object;
        }

        float result;
        float.TryParse(Object.ToString(), out result);
        return result;
    }

    public float[] GetFloatArray()
    {
        return Object is float[] ? (float[])Object : new float[4];
    }

    public double GetDouble()
    {
        if (Object is double)
        {
            return (double)Object;
        }

        double result;
        double.TryParse(Object.ToString(), out result);
        return result;
    }

    public DataItem GetPointer()
    {
        return Object is DataItem ? (DataItem)Object : null;
    }

    public Color GetColor()
    {
        return Object is Color ? (Color)Object : getColorFromString(ToString());
    }

    private static Color getColorFromString(string text)
    {
        var color = new Color();
        var strArray1 = new string[0];
        var str = "";
        if (text != null)
        {
            var separator = new string[1] {":"};
            var strArray2 = text.Split(separator, StringSplitOptions.None);
            if (strArray2.Length >= 2)
            {
                text = strArray2.First();
                str = strArray2.Last();
            }

            strArray1 = text.Split(',');
        }

        for (var index = 0; index < strArray1.Length && index < 4; ++index)
        {
            if (str == "sRGB")
            {
                byte result;
                if (byte.TryParse(strArray1[index], out result))
                {
                    switch (index)
                    {
                        case 0:
                            color.R = result;
                            continue;
                        case 1:
                            color.G = result;
                            continue;
                        case 2:
                            color.B = result;
                            continue;
                        case 3:
                            color.A = result;
                            continue;
                        default:
                            continue;
                    }
                }
            }
            else
            {
                float result;
                if (float.TryParse(strArray1[index], out result))
                {
                    switch (index)
                    {
                        case 0:
                            color.ScR = result;
                            continue;
                        case 1:
                            color.ScG = result;
                            continue;
                        case 2:
                            color.ScB = result;
                            continue;
                        case 3:
                            color.ScA = result;
                            continue;
                        default:
                            continue;
                    }
                }
            }
        }

        return color;
    }

    public override string ToString()
    {
        if (Object == null)
        {
            return "";
        }

        switch (PrimitiveType)
        {
            case PrimitiveType.Float4:
                if (Object is float[])
                {
                    var numArray = (float[])Object;
                    return numArray[0] + "," + numArray[1] + "," + numArray[2] + "," + numArray[3];
                }

                break;
            case PrimitiveType.Color:
                if (Object is Color)
                {
                    var color = (Color)Object;
                    return color.ScR + "," + color.ScG + "," + color.ScB + "," + color.ScA;
                }

                break;
            case PrimitiveType.Pointer:
                if (Object is DataItem)
                {
                    return ((DataItem)Object).FullPath.ToString();
                }

                break;
        }

        return Object.ToString();
    }

    public string ToStringWithFormat(Field field, string format)
    {
        if (Object == null)
        {
            return "";
        }

        if (format != null && format != "")
        {
            switch (PrimitiveType)
            {
                case PrimitiveType.Bool:
                    return string.Format(format, (bool)Object);
                case PrimitiveType.String:
                    return string.Format(format, (string)Object);
                case PrimitiveType.Char:
                    return string.Format(format, (sbyte)Object);
                case PrimitiveType.UChar:
                    return string.Format(format, (byte)Object);
                case PrimitiveType.Short:
                    return string.Format(format, (short)Object);
                case PrimitiveType.UShort:
                    return string.Format(format, (ushort)Object);
                case PrimitiveType.Int:
                    return string.Format(format, (int)Object);
                case PrimitiveType.UInt:
                    return string.Format(format, (uint)Object);
                case PrimitiveType.Long:
                    return string.Format(format, (long)Object);
                case PrimitiveType.ULong:
                    return string.Format(format, (ulong)Object);
                case PrimitiveType.Float:
                    return string.Format(format, (float)Object);
                case PrimitiveType.Double:
                    return string.Format(format, (double)Object);
                case PrimitiveType.Float4:
                    var numArray1 = (float[])Object;
                    return string.Format(format, numArray1[0], numArray1[1], numArray1[2], numArray1[3]);
            }
        }

        switch (PrimitiveType)
        {
            case PrimitiveType.Float4:
                if (Object is float[])
                {
                    var numArray2 = (float[])Object;
                    format = "F2";
                    return "X=" + numArray2[0].ToString(format) + " Y=" + numArray2[1].ToString(format) + " Z=" +
                           numArray2[2].ToString(format) + "\n W=" + numArray2[3].ToString(format);
                }

                break;
            case PrimitiveType.Enum:
                var dataType = field.DataType as Enum;
                var str = Object as string;
                return (GetEnum_DisplayNameObject(dataType, str) ?? str).ToString();
        }

        return ToString();
    }

    public override bool Equals(object obj)
    {
        if (obj is Value obj1 && PrimitiveType == obj1.PrimitiveType)
        {
            switch (PrimitiveType)
            {
                case PrimitiveType.Bool:
                    return (bool)Object == (bool)obj1.Object;
                case PrimitiveType.String:
                    return (string)Object == (string)obj1.Object;
                case PrimitiveType.Char:
                    return (sbyte)Object == (sbyte)obj1.Object;
                case PrimitiveType.UChar:
                    return (byte)Object == (byte)obj1.Object;
                case PrimitiveType.Short:
                    return (short)Object == (short)obj1.Object;
                case PrimitiveType.UShort:
                    return (ushort)Object == (ushort)obj1.Object;
                case PrimitiveType.Int:
                    return (int)Object == (int)obj1.Object;
                case PrimitiveType.UInt:
                    return (int)(uint)Object == (int)(uint)obj1.Object;
                case PrimitiveType.Long:
                    return (long)Object == (long)obj1.Object;
                case PrimitiveType.ULong:
                    return (long)(ulong)Object == (long)(ulong)obj1.Object;
                case PrimitiveType.Float:
                    return (float)Object == (double)(float)obj1.Object;
                case PrimitiveType.Double:
                    return (double)Object == (double)obj1.Object;
                case PrimitiveType.Float4:
                    var numArray1 = (float[])Object;
                    var numArray2 = (float[])obj1.Object;
                    for (var index = 0; index < 4; ++index)
                    {
                        if (numArray1[index] != (double)numArray2[index])
                        {
                            return false;
                        }
                    }

                    return true;
                case PrimitiveType.Color:
                    return (Color)Object == (Color)obj1.Object;
                case PrimitiveType.Pointer:
                    return (DataItem)Object == (DataItem)obj1.Object;
                case PrimitiveType.Enum:
                    return Object == obj1.Object;
            }
        }

        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return Object != null ? Object.GetHashCode() : base.GetHashCode();
    }

    private enum ConvertEnumType
    {
        CET_DISPLAYNAME_2_ENUMNAME,
        CET_ENUMNAME_2_DISPLAYNAME
    }
}