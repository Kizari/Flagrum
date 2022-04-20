using System;
using System.Numerics;
using System.Text;

namespace Flagrum.Core.Utilities;

public static class Extensions
{
    public static float[,] ToArray(this Matrix4x4 matrix)
    {
        return new[,]
        {
            {matrix.M11, matrix.M12, matrix.M13, matrix.M14},
            {matrix.M21, matrix.M22, matrix.M23, matrix.M24},
            {matrix.M31, matrix.M32, matrix.M33, matrix.M34},
            {matrix.M41, matrix.M42, matrix.M43, matrix.M44}
        };
    }

    public static string ToBase64(this string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(bytes);
    }

    public static string FromBase64(this string input)
    {
        var bytes = Convert.FromBase64String(input);
        return Encoding.UTF8.GetString(bytes);
    }
}