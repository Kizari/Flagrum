namespace Flagrum.Core.Physics.Collision.PhysX;

public enum PhysicsConcreteType : ushort
{
    eUNDEFINED,
    eHEIGHTFIELD,
    eCONVEX_MESH,
    eTRIANGLE_MESH,
    eCLOTH_FABRIC,
    eRIGID_DYNAMIC,
    eRIGID_STATIC,
    eSHAPE,
    eMATERIAL,
    eCONSTRAINT,
    eCLOTH,
    ePARTICLE_SYSTEM,
    ePARTICLE_FLUID,
    eAGGREGATE,
    eARTICULATION,
    eARTICULATION_LINK,
    eARTICULATION_JOINT,
    ePHYSX_CORE_COUNT,
    eFIRST_PHYSX_EXTENSION = 256,
    eFIRST_VEHICLE_EXTENSION = 512,
    eFIRST_USER_EXTENSION = 1024
}