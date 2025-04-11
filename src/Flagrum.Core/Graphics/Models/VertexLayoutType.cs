namespace Flagrum.Core.Graphics.Models;

public enum VertexLayoutType
{
    NULL = 0x0,
    Skinning_4Bones = 0x1,
    Skinning_8Bones = 0x2,
    NTB2 = 0x4,
    Skinning_1Bones = 0x8,
    Skinning_6Bones = 0x10,
    BoneIndices16 = 0x20,

    // This is a test entry for Comrades, the name is an assumption
    BoneIndices32 = 0x21,

    // This is a test entry for Comrades, the name is an assumption
    BoneIndices64 = 0x22,
    Skinning_Any = 0x1B
}