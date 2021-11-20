using System;
using System.Collections.Generic;

namespace Flagrum.Gfxbin.Gmdl.Constructs;

public struct UVMap
{
    public IList<UV> UVs;
}

public struct UV
{
    public Half U;
    public Half V;
}

/// <summary>
///     Intermediary class as JSON.NET doesn't handle serializing Half values properly
/// </summary>
public class UVMap32
{
    public List<UV32> UVs;
}

/// <summary>
///     Intermediary class as JSON.NET doesn't handle serializing Half values properly
/// </summary>
public class UV32
{
    public float U { get; set; }
    public float V { get; set; }
}