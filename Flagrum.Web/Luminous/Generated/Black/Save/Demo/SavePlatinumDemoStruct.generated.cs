//----------------------
// <auto-generated>
// This file was automatically generated. Any changes to it will be lost if and when the file is regenerated.
// </auto-generated>
//----------------------
#pragma warning disable

using System;
using SQEX.Luminous.Core.Object;
using System.Collections.Generic;
using CodeDom = System.CodeDom;

namespace Black.Save.Demo
{
    [Serializable, CodeDom.Compiler.GeneratedCode("Luminaire", "0.1")]
    public partial class SavePlatinumDemoStruct
    {
        new public static ObjectType ObjectType { get; private set; }
        private static PropertyContainer fieldProperties;
		
		public sbyte carbuncleName_;
		public bool isClear_;
		public long total_seconds;
		
        
        new public static void SetupObjectType()
        {
            if (ObjectType != null)
            {
                return;
            }

            var dummy = new SavePlatinumDemoStruct();
            var properties = dummy.GetFieldProperties();

            ObjectType = new ObjectType("Black.Save.Demo.SavePlatinumDemoStruct", 0, Black.Save.Demo.SavePlatinumDemoStruct.ObjectType, null, properties, 0, 272);
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

            fieldProperties = new PropertyContainer("Black.Save.Demo.SavePlatinumDemoStruct", null, -1620242067, -1914306695);
            
			
			
			fieldProperties.AddProperty(new Property("carbuncleName_", 4137410360, "char", 0, 256, 256, Property.PrimitiveType.Int8, 0, (char)8));
			fieldProperties.AddProperty(new Property("isClear_", 3723232503, "bool", 256, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
			fieldProperties.AddProperty(new Property("total_seconds", 443148979, "int64_t", 264, 8, 1, Property.PrimitiveType.Int64, 0, (char)0));
			
			
			return fieldProperties;
        }

		
    }
}