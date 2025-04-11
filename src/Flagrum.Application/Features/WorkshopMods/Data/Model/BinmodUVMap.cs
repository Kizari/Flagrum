using System.Collections.Generic;

namespace Flagrum.Application.Features.WorkshopMods.Data.Model;

/// <summary>
/// Intermediary class as JSON.NET doesn't handle serializing Half values properly
/// </summary>
public class BinmodUVMap32
{
    public List<BinmodUV32> UVs;
}

/// <summary>
/// Intermediary class as JSON.NET doesn't handle serializing Half values properly
/// </summary>
public class BinmodUV32
{
    public float U { get; set; }
    public float V { get; set; }
}