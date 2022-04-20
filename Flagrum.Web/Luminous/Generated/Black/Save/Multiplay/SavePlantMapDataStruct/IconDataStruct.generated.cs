using SQEX.Luminous.Core.Object;
using System.Collections.Generic;

/*
namespace Black.Save.Multiplay.SavePlantMapDataStruct
{
    public partial class IconDataStruct
    {
        new public static ObjectType ObjectType { get; private set; }
        private static PropertyContainer fieldProperties;
		
		public uint id_;
		public sbyte state_;
		
        
        new public static void SetupObjectType()
        {
            if (ObjectType != null)
            {
                return;
            }

            var dummy = new IconDataStruct();
            var properties = dummy.GetFieldProperties();

            ObjectType = new ObjectType("Black.Save.Multiplay.SavePlantMapDataStruct.IconDataStruct", 0, Black.Save.Multiplay.SavePlantMapDataStruct.IconDataStruct.ObjectType, null, properties, 0, 8);
        }
		
        public  ObjectType GetObjectType()
        {
            return ObjectType;
        }

        protected  PropertyContainer GetFieldProperties()
        {
            if (fieldProperties != null)
            {
                return fieldProperties;
            }

            fieldProperties = new PropertyContainer("Black.Save.Multiplay.SavePlantMapDataStruct.IconDataStruct", null, 727374762, 1653884926);
            
			
			
			fieldProperties.AddProperty(new Property("id_", 2899315373, "SQEX.Ebony.Std.Fixid", 0, 4, 1, Property.PrimitiveType.Fixid, 0, (char)0));
			fieldProperties.AddProperty(new Property("state_", 3732062219, "uint8_t", 4, 1, 1, Property.PrimitiveType.Int8, 0, (char)0));
			
			
			return fieldProperties;
        }

		
    }
}*/