using SQEX.Luminous.Core.Object;
using System.Collections.Generic;

namespace SQEX.Ebony.Framework.Entity
{
    public partial class EntityPackage : EntityPackageReference
    {
        public string simpleName_;
        /*
        new public static ObjectType ObjectType { get; private set; }
        private static PropertyContainer fieldProperties;
        
        public string simpleName_;
        public List<Luminous.Core.Object.Object> loadedObjects_ = new List<Luminous.Core.Object.Object>();
        public List<string> loadedObjectNames_ = new List<string>();
        public List<string> loadedObjectPaths_ = new List<string>();
        public int sequenceUpdateOrderPreset_;
        public ushort sequenceUpdateOrderDirect_;

        new public static void SetupObjectType()
        {
            if (ObjectType != null)
            {
                return;
            }

            var dummy = new EntityPackage();
            var properties = dummy.GetFieldProperties();

            ObjectType = new ObjectType("SQEX.Ebony.Framework.Entity.EntityPackage", 0, EntityPackageReference.ObjectType, Construct, properties, 8, 592);
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

            fieldProperties = new PropertyContainer("SQEX.Ebony.Framework.Entity.EntityPackage", base.GetFieldProperties(), -1610658500, -1708356512);
            fieldProperties.AddIndirectlyProperty(new Property("entities_", 798990575, "SQEX.Ebony.Std.IntrusivePointerDynamicArray< SQEX.Ebony.Framework.Entity.Entity >", 64, 16, 1, Property.PrimitiveType.IntrusivePointerArray, 0, (char)5));
            fieldProperties.AddIndirectlyProperty(new Property("hasTransform_", 3096138238, "bool", 88, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
            fieldProperties.AddIndirectlyProperty(new Property("position_", 987254735, "Luminous.Math.VectorA", 96, 16, 1, Property.PrimitiveType.Vector4, 0, (char)0));
            fieldProperties.AddIndirectlyProperty(new Property("rotation_", 36328192, "Luminous.Math.VectorA", 112, 16, 1, Property.PrimitiveType.Vector4, 0, (char)0));
            fieldProperties.AddIndirectlyProperty(new Property("scaling_", 3325430311, "float", 128, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
            fieldProperties.AddIndirectlyProperty(new Property("canManipulate_", 3989276646, "bool", 132, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));

            fieldProperties.AddIndirectlyProperty(new Property("sourcePath_", 341055184, "Base.String", 208, 16, 1, Property.PrimitiveType.String, 0, (char)0));
            fieldProperties.AddIndirectlyProperty(new Property("name_", 182823483, "Base.String", 224, 16, 1, Property.PrimitiveType.String, 0, (char)0));
            fieldProperties.AddIndirectlyProperty(new Property("isTemplateTraySourceReference_", 3775626232, "bool", 240, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
            fieldProperties.AddIndirectlyProperty(new Property("isShared_", 3455118081, "bool", 241, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
            fieldProperties.AddIndirectlyProperty(new Property("startupLoad_", 3202049383, "bool", 242, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));

            fieldProperties.AddProperty(new Property("loadedObjects_", 4286216873, "SQEX.Ebony.Std.IntrusivePointerDynamicArray< Base.Object.Object, MEMORY_CATEGORY_FRAMEWORK >", 304, 16, 1, Property.PrimitiveType.IntrusivePointerArray, 0, (char)0));
            fieldProperties.AddProperty(new Property("loadedObjectNames_", 503273934, "SQEX.Ebony.Std.IntrusivePointerDynamicArray< Base.Object.String, MEMORY_CATEGORY_FRAMEWORK >", 320, 16, 1, Property.PrimitiveType.Array, 0, (char)0));
            fieldProperties.AddProperty(new Property("loadedObjectPaths_", 453683170, "SQEX.Ebony.Std.IntrusivePointerDynamicArray< Base.Object.String, MEMORY_CATEGORY_FRAMEWORK >", 336, 16, 1, Property.PrimitiveType.Array, 0, (char)0));
            fieldProperties.AddProperty(new Property("sequenceUpdateOrderPreset_", 2630853337, "SQEX.Ebony.Framework.Entity.EntityPackage.SequenceUpdateOrderPreset", 352, 4, 1, Property.PrimitiveType.Enum, 0, (char)0));
            fieldProperties.AddProperty(new Property("sequenceUpdateOrderDirect_", 3519507007, "uint16_t", 356, 2, 1, Property.PrimitiveType.UInt16, 0, (char)0));
            fieldProperties.AddProperty(new Property("entityPackageSharedChildPathList_", 1047969541, "SQEX.Ebony.Std.DynamicArray< SQEX.Ebony.Framework.Entity.EntityPackageSharedRefItem*, MEMORY_CATEGORY_FRAMEWORK >", 360, 16, 1, Property.PrimitiveType.PointerArray, 0, (char)0));
            fieldProperties.AddProperty(new Property("packageSearchLabelId_", 1572444187, "SQEX.Ebony.Std.Fixid", 376, 4, 1, Property.PrimitiveType.Fixid, 0, (char)0));
            fieldProperties.AddProperty(new Property("bUmbraBakeTarget_", 26211871, "bool", 380, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
            fieldProperties.AddProperty(new Property("bForbidDuplicatedLoad_", 2326256737, "bool", 381, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));

            return fieldProperties;
        }

        private static BaseObject Construct()
        {
            return new EntityPackage();
        }
        */

        public void AddLoadedObject(Luminous.Core.Object.Object obj, string loadedObjectName, string loadedObjectPath)
        {
            this.loadedObjects_.Add(obj);
            this.loadedObjectNames_.Add(loadedObjectName);
            this.loadedObjectPaths_.Add(loadedObjectPath);
        }

        public IDictionary<string, Object> GetLoadedObjectsAndNames()
        {
            var result = new Dictionary<string, Object>();
            for (var i = 0; i < this.loadedObjects_.Count; i++)
            {
                // FIXME why does this happen?
                if (result.ContainsKey(this.loadedObjectNames_[i]))
                {
                    continue;
                }
                result.Add(this.loadedObjectNames_[i], this.loadedObjects_[i]);
            }

            return result;
        }
    }
}