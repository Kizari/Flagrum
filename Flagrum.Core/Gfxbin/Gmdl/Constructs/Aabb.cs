﻿namespace Flagrum.Core.Gfxbin.Gmdl.Constructs;

public class Aabb
{
    public Aabb(Vector3 min, Vector3 max)
    {
        Min = min;
        Max = max;
    }

    public Vector3 Min { get; }
    public Vector3 Max { get; }
}