namespace Flagrum.Interop;

public unsafe struct CModelData
{
    public int* BoneIds;
    public char** BoneNames;
    public CMeshData* Meshes;
}

public unsafe struct CMeshData
{
    public CColorMap* ColorMaps;
    public CFace* Faces;
    public char* Name;
    public CVector4* Normals;
    public CUVMap* UVMaps;
    public CVector3* VertexPositions;
    public CWeightMap* WeightMaps;
}

public struct CVertexWeight
{
    public int BoneId;
    public float Weight;
}

public unsafe struct CVertexWeights
{
    public CVertexWeight* VertexWeight;
}

public unsafe struct CWeightMap
{
    public CVertexWeights* VertexWeights;
}

public unsafe struct CVector3
{
    public float* XYZ;
}

public unsafe struct CUVMap
{
    public CVector2* Coords;
}

public unsafe struct CVector2
{
    public float* UV;
}

public unsafe struct CVector4
{
    public sbyte* XYZW;
}

public unsafe struct CFace
{
    public int* Indices;
}

public unsafe struct CColorMap
{
    public CColor4* Colors;
}

public unsafe struct CColor4
{
    public byte* RGBA;
}