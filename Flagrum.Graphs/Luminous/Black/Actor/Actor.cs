
namespace SQEX.Luminous.Core.Object
{
    public partial class Object : Luminous.Core.Object.BaseObject
    {

    }
}

namespace SQEX.Ebony.Framework.Entity
{
    public partial class Entity : SQEX.Luminous.Core.Object.Object
    {

    }
}

namespace Black.Actor
{
    public partial class Actor : SQEX.Ebony.Framework.Entity.Entity
    {
        public interface IConstraintCustomOption
        {
        }

        public class ConstraintInfo
        {
            public bool constraintPhysicsFlag;
            public UnityEngine.Vector4 constraintLocalOffsetPosition;
            public UnityEngine.Vector4 constraintLocalOffsetRotation;
            public float constraintBaseScale;
            public Black.Actor.ActorReference constraintActor;
            public int constraintHierarchyIndex;
            public uint constraintFixid;
            public uint constraintFlag;
            public Black.Actor.Actor.IConstraintCustomOption constraintCustomInfo_;
        }
    }
}