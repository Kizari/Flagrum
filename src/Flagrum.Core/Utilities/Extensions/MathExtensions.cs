using System;
using System.Drawing;
using System.Numerics;
using Flagrum.Core.Utilities.Type;

namespace Flagrum.Core.Utilities.Extensions;

public static class MathExtensions
{
    public static float GetValue(this Matrix4x4 matrix, int row, int column)
    {
        return row switch
        {
            0 => column switch
            {
                0 => matrix.M11,
                1 => matrix.M12,
                2 => matrix.M13,
                3 => matrix.M14,
                _ => throw new IndexOutOfRangeException()
            },
            1 => column switch
            {
                0 => matrix.M21,
                1 => matrix.M22,
                2 => matrix.M23,
                3 => matrix.M24,
                _ => throw new IndexOutOfRangeException()
            },
            2 => column switch
            {
                0 => matrix.M31,
                1 => matrix.M32,
                2 => matrix.M33,
                3 => matrix.M34,
                _ => throw new IndexOutOfRangeException()
            },
            3 => column switch
            {
                0 => matrix.M41,
                1 => matrix.M42,
                2 => matrix.M43,
                3 => matrix.M44,
                _ => throw new IndexOutOfRangeException()
            },
            _ => throw new IndexOutOfRangeException()
        };
    }

    public static void SetValue(this ref Matrix4x4 matrix, int row, int column, float value)
    {
        switch (row)
        {
            case 0:
                switch (column)
                {
                    case 0:
                        matrix.M11 = value;
                        break;
                    case 1:
                        matrix.M12 = value;
                        break;
                    case 2:
                        matrix.M13 = value;
                        break;
                    case 3:
                        matrix.M14 = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }

                break;
            case 1:
                switch (column)
                {
                    case 0:
                        matrix.M21 = value;
                        break;
                    case 1:
                        matrix.M22 = value;
                        break;
                    case 2:
                        matrix.M23 = value;
                        break;
                    case 3:
                        matrix.M24 = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }

                break;
            case 2:
                switch (column)
                {
                    case 0:
                        matrix.M31 = value;
                        break;
                    case 1:
                        matrix.M32 = value;
                        break;
                    case 2:
                        matrix.M33 = value;
                        break;
                    case 3:
                        matrix.M34 = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }

                break;
            case 3:
                switch (column)
                {
                    case 0:
                        matrix.M41 = value;
                        break;
                    case 1:
                        matrix.M42 = value;
                        break;
                    case 2:
                        matrix.M43 = value;
                        break;
                    case 3:
                        matrix.M44 = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }

                break;
            default:
                throw new IndexOutOfRangeException();
        }
    }

    public static float GetValue(this Vector4 vector, int axis)
    {
        switch (axis)
        {
            case 0:
                return vector.X;
            case 1:
                return vector.Y;
            case 2:
                return vector.Z;
            case 3:
                return vector.W;
        }

        throw new ArgumentOutOfRangeException(nameof(axis));
    }

    public static Color ToRandomColor(this string name)
    {
        var hue = name.GetHashCode() % 360;
        return ColorRGB.FromHSL(hue / 360.0, 0.6, 0.6);
    }

    public static string ToHex(this Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    public static int RoundToMultiple(this double value, int multiple)
    {
        return (int) Math.Round(value / multiple) * multiple;
    }
}