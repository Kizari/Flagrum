namespace Flagrum.Core.Physics.Collision;

public enum PhysicsBlockType
{
    BLOCK_TYPE_NULL = 0x0,
    BLOCK_TYPE_STATIC_BODY = 0x1,
    BLOCK_TYPE_OVERLAP = 0x2,
    BLOCK_TYPE_RAGDOLL = 0x3,
    BLOCK_TYPE_CHAIN = 0x4,
    BLOCK_TYPE_CLOTH = 0x5,
    BLOCK_TYPE_CHARACTER_PROXY = 0x6,
    BLOCK_TYPE_IK = 0x7,
    BLOCK_TYPE_CLIPPING = 0x8,
    BLOCK_TYPE_WORLD_WIND = 0x9,
    BLOCK_TYPE_ANIMATED_SHAPE = 0xA,
    BLOCK_TYPE_DYNAMIC = 0xB
}