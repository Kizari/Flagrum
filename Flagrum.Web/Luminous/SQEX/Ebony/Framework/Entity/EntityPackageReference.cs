﻿using SQEX.Luminous.Core.Object;

namespace SQEX.Ebony.Framework.Entity
{
    public partial class EntityPackageReference : EntityGroup
    {/*
        new public static ObjectType ObjectType { get; private set; }
        private static PropertyContainer fieldProperties;

        public string sourcePath_ { get; set;  }
        public string name_ { get; set; }
        public bool isTemplateTraySourceReference_ { get; set; }
        public bool isShared_ { get; set; }
        public bool startupLoad_ { get; set; }

        new public static void SetupObjectType()
        {
            if (ObjectType != null)
            {
                return;
            }

            var dummy = new EntityPackageReference();
            var properties = dummy.GetFieldProperties();

            ObjectType = new ObjectType("SQEX.Ebony.Framework.Entity.EntityPackageReference", 0, EntityGroup.ObjectType, Construct, properties, 1, 256);
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

            fieldProperties = new PropertyContainer("SQEX.Ebony.Framework.Entity.EntityPackageReference", base.GetFieldProperties(), 276846282, 1251993403);
            fieldProperties.AddIndirectlyProperty(new Property("entities_", 798990575, "SQEX.Ebony.Std.IntrusivePointerDynamicArray< SQEX.Ebony.Framework.Entity.Entity >", 64, 16, 1, Property.PrimitiveType.IntrusivePointerArray, 0, (char)5));
            fieldProperties.AddIndirectlyProperty(new Property("hasTransform_", 3096138238, "bool", 88, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
            fieldProperties.AddIndirectlyProperty(new Property("position_", 987254735, "Luminous.Math.VectorA", 96, 16, 1, Property.PrimitiveType.Vector4, 0, (char)0));
            fieldProperties.AddIndirectlyProperty(new Property("rotation_", 36328192, "Luminous.Math.VectorA", 112, 16, 1, Property.PrimitiveType.Vector4, 0, (char)0));
            fieldProperties.AddIndirectlyProperty(new Property("scaling_", 3325430311, "float", 128, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
            fieldProperties.AddIndirectlyProperty(new Property("canManipulate_", 3989276646, "bool", 132, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));

            fieldProperties.AddProperty(new Property("sourcePath_", 341055184, "Base.String", 208, 16, 1, Property.PrimitiveType.String, 0, (char)0));
            fieldProperties.AddProperty(new Property("name_", 182823483, "Base.String", 224, 16, 1, Property.PrimitiveType.String, 0, (char)0));
            fieldProperties.AddProperty(new Property("isTemplateTraySourceReference_", 3775626232, "bool", 240, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
            fieldProperties.AddProperty(new Property("isShared_", 3455118081, "bool", 241, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
            fieldProperties.AddProperty(new Property("startupLoad_", 3202049383, "bool", 242, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
            return fieldProperties;
        }

        private static BaseObject Construct()
        {
            return new EntityPackageReference();
        }*/
    }
}
