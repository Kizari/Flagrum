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

namespace Black.Entity
{
    [Serializable, CodeDom.Compiler.GeneratedCode("Luminaire", "0.1")]
    public partial class WaterExclusionBoxParameter : BaseObject
    {
        new public static ObjectType ObjectType { get; private set; }
        private static PropertyContainer fieldProperties;
		
		public UnityEngine.Vector4 localPosition_;
		public UnityEngine.Vector4 scale_;
		
        
        new public static void SetupObjectType()
        {
            if (ObjectType != null)
            {
                return;
            }

            var dummy = new WaterExclusionBoxParameter();
            var properties = dummy.GetFieldProperties();

            ObjectType = new ObjectType("Black.Entity.WaterExclusionBoxParameter", 0, Black.Entity.WaterExclusionBoxParameter.ObjectType, Construct, properties, 0, 48);
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

            fieldProperties = new PropertyContainer("Black.Entity.WaterExclusionBoxParameter", base.GetFieldProperties(), -1818517810, -951594638);
            
			
			
			fieldProperties.AddProperty(new Property("localPosition_", 355810764, "Luminous.Math.VectorA", 16, 16, 1, Property.PrimitiveType.Vector4, 0, (char)0));
			fieldProperties.AddProperty(new Property("scale_", 3252771946, "Luminous.Math.VectorA", 32, 16, 1, Property.PrimitiveType.Vector4, 0, (char)0));
			
			
			return fieldProperties;
        }

		
        private static BaseObject Construct()
        {
            return new WaterExclusionBoxParameter();
        }
		
    }
}