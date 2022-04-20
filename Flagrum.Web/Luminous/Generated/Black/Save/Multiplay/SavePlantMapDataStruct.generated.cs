using SQEX.Luminous.Core.Object;
using System.Collections.Generic;

namespace Black.Save.Multiplay
{
    public partial class SavePlantMapDataStruct
    {
        new public static ObjectType ObjectType { get; private set; }
        private static PropertyContainer fieldProperties;
		
		public bool isCompletedLesta_;
		//public Black.Save.Multiplay.SavePlantMapDataStruct.IconDataStruct iconDataList_;
		
        
        new public static void SetupObjectType()
        {
            if (ObjectType != null)
            {
                return;
            }

            var dummy = new SavePlantMapDataStruct();
            var properties = dummy.GetFieldProperties();

            ObjectType = new ObjectType("Black.Save.Multiplay.SavePlantMapDataStruct", 0, Black.Save.Multiplay.SavePlantMapDataStruct.ObjectType, null, properties, 0, 4004);
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

            fieldProperties = new PropertyContainer("Black.Save.Multiplay.SavePlantMapDataStruct", null, -751665946, -1251616530);
            
			
			
			fieldProperties.AddProperty(new Property("isCompletedLesta_", 2026538682, "bool", 0, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
			fieldProperties.AddProperty(new Property("iconDataList_", 4202595757, "Black.Save.Multiplay.SavePlantMapDataStruct.IconDataStruct", 4, 4000, 500, Property.PrimitiveType.ClassField, 0, (char)8));
			
			
			return fieldProperties;
        }

		
    }
}