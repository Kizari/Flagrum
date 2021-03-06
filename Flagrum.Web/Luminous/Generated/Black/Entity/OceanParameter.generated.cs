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
    public partial class OceanParameter : BaseObject
    {
        new public static ObjectType ObjectType { get; private set; }
        private static PropertyContainer fieldProperties;
		
		public int waveIntensityLevel_;
		public UnityEngine.Vector4 fftOctave0Params0_;
		public UnityEngine.Vector4 fftOctave0Params1_;
		public UnityEngine.Vector4 fftOctave1Params0_;
		public UnityEngine.Vector4 fftOctave1Params1_;
		public UnityEngine.Vector4 foamParams_;
		
        
        new public static void SetupObjectType()
        {
            if (ObjectType != null)
            {
                return;
            }

            var dummy = new OceanParameter();
            var properties = dummy.GetFieldProperties();

            ObjectType = new ObjectType("Black.Entity.OceanParameter", 0, Black.Entity.OceanParameter.ObjectType, Construct, properties, 0, 96);
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

            fieldProperties = new PropertyContainer("Black.Entity.OceanParameter", base.GetFieldProperties(), 432306394, -698696659);
            
			
			
			fieldProperties.AddProperty(new Property("waveIntensityLevel_", 1325417958, "int", 8, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddProperty(new Property("fftOctave0Params0_", 2539591154, "Luminous.Math.VectorA", 16, 16, 1, Property.PrimitiveType.Vector4, 0, (char)0));
			fieldProperties.AddProperty(new Property("fftOctave0Params1_", 2539444059, "Luminous.Math.VectorA", 32, 16, 1, Property.PrimitiveType.Vector4, 0, (char)0));
			fieldProperties.AddProperty(new Property("fftOctave1Params0_", 3672044429, "Luminous.Math.VectorA", 48, 16, 1, Property.PrimitiveType.Vector4, 0, (char)0));
			fieldProperties.AddProperty(new Property("fftOctave1Params1_", 3672191524, "Luminous.Math.VectorA", 64, 16, 1, Property.PrimitiveType.Vector4, 0, (char)0));
			fieldProperties.AddProperty(new Property("foamParams_", 385427663, "Luminous.Math.VectorA", 80, 16, 1, Property.PrimitiveType.Vector4, 0, (char)0));
			
			
			return fieldProperties;
        }

		
        private static BaseObject Construct()
        {
            return new OceanParameter();
        }
		
    }
}