using SQEX.Luminous.Core.Object;

namespace Black.Entity
{
    public partial class EntityPackage : SQEX.Ebony.Framework.Entity.EntityPackage
    {/*
        new public static ObjectType ObjectType { get; private set; }
        private static PropertyContainer fieldProperties;
        
        new public static void SetupObjectType()
        {
            if (ObjectType != null)
            {
                return;
            }

            var dummy = new EntityPackage();
            var properties = dummy.GetFieldProperties();

            ObjectType = new ObjectType("Black.Entity.EntityPackage", 0, Ebony.Framework.Entity.EntityPackage.ObjectType, Construct, properties, 1, 208);
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

            fieldProperties = new PropertyContainer("Black.Entity.EntityPackage", base.GetFieldProperties(), -1194656680, 1792541562);
            return fieldProperties;
        }

        private static BaseObject Construct()
        {
            return new EntityPackage();
        }*/
    }
}
