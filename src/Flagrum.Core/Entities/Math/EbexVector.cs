using System;

namespace Flagrum.Core.Scripting.Ebex.Math;

public struct Vector
{
    public readonly float X;
    public readonly float Y;
    public readonly float Z;
    public readonly float W;

    public Vector(float x, float y, float z, float w = 1f)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public Vector(double x, double y, double z, double w = 1.0)
    {
        X = (float)x;
        Y = (float)y;
        Z = (float)z;
        W = (float)w;
    }

    public static Vector operator *(Vector vec, float t)
    {
        throw new NotImplementedException();
    }
}