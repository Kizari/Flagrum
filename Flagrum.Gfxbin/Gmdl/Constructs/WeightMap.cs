using System.Collections.Generic;

namespace Flagrum.Gfxbin.Gmdl.Constructs;

public struct WeightMap
{
    public IList<VertexWeights> VertexWeights;
}

public struct VertexWeights
{
    public IList<VertexWeight> Values;
}

public struct VertexWeight
{
    public int BoneIndex;
    public byte Weight;
}