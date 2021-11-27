using SQEX.Luminous.Core.Object;

namespace SQEX.Ebony.Framework.Entity
{
    public partial class Entity : SQEX.Luminous.Core.Object.Object
    {/*
        new public static ObjectType ObjectType { get; private set; }
        private static PropertyContainer fieldProperties;

        new public static void SetupObjectType()
        {
            if (ObjectType != null)
            {
                return;
            }

            var dummy = new Entity();
            var properties = dummy.GetFieldProperties();

            ObjectType = new ObjectType("SQEX.Ebony.Framework.Entity.Entity", 0, Object.ObjectType, Construct, properties, 1, 64);
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

            fieldProperties = new PropertyContainer("SQEX.Ebony.Framework.Entity.Entity", base.GetFieldProperties(), -1268393468, 1324801635);
            return fieldProperties;
        }
        private static BaseObject Construct()
        {
            return new Entity();
        }*/
    }
}
