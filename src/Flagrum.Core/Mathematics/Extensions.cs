using System;

namespace Flagrum.Core.Mathematics;

public static class Extensions
{
    public static float ToRadians(this float degrees)
    {
        return (float)(Math.PI / 180 * degrees);
    }
}