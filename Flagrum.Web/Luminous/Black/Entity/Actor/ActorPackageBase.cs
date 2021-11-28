using SQEX.Luminous.Core.Object;

namespace Black.Entity.Actor
{
    public partial class ActorPackageBase : Black.Entity.EntityPackage
    {/*
        new public static ObjectType ObjectType { get; private set; }
        private static PropertyContainer fieldProperties;
        
        new public static void SetupObjectType()
        {
            if (ObjectType != null)
            {
                return;
            }

            var dummy = new ActorPackageBase();
            var properties = dummy.GetFieldProperties();

            ObjectType = new ObjectType("Black.Entity.Actor.ActorPackageBase", 0, Black.Entity.EntityPackage.ObjectType, Construct, properties, 1, 624);
        }

        public override ObjectType GetObjectType()
        {
            return ObjectType;
        }

        protected override PropertyContainer GetFieldProperties()
        {
            if (fieldProperties != null)
            {
                return fieldProperties;
            }

            fieldProperties = new PropertyContainer("Black.Entity.Actor.ActorPackageBase", base.GetFieldProperties(), -1936346568, 174207750);
            return fieldProperties;
        }

        private static BaseObject Construct()
        {
            return new ActorPackageBase();
        }*/
    }
}
