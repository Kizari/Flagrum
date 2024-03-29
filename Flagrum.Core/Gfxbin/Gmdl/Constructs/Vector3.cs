﻿namespace Flagrum.Core.Gfxbin.Gmdl.Constructs;

public struct Vector3
{
    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public float X;
    public float Y;
    public float Z;

    public override string ToString()
    {
        return $"[{X}, {Y}, {Z}]";
    }
}