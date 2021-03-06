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

namespace Black.Sequence.ScreenEffect
{
    [Serializable, CodeDom.Compiler.GeneratedCode("Luminaire", "0.1")]
    public partial class VolLightFilterSequenceData : BaseObject
    {
        new public static ObjectType ObjectType { get; private set; }
        private static PropertyContainer fieldProperties;
		
		public bool enabled_;
		public bool enableWeather_;
		public float zNear_;
		public float zFar_;
		public float fogThickness_;
		public float mainlightPower_;
		public float locallightPower_;
		public float ambientPower_;
		public float emissivePower_;
		public UnityEngine.Color tint_;
		public UnityEngine.Color emissiveColor_;
		public float ambientCubemapPower_;
		public float ambientCubemapTenPercentHeight_;
		public float ambientCubemapStartY_;
		public float ambientDiffuseLightProbe_;
		public bool noiseEnabled_;
		public float noiseAmount_;
		public float noisePeriod_;
		public float noiseScrollSpeed_;
		public float noiseScrollRot_;
		public float noiseScrollTilt_;
		public bool heightFogEnabled_;
		public float tenPercentHeight_;
		public float startY_;
		public float playerRelative_;
		public float fadeInDistance_;
		public float fadeInToe_;
		public float fadeInShoulder_;
		public int blurPassCountVSM_;
		public float nearFarContrast_;
		public float extinction_;
		public bool farFogEnabled_;
		public float farFogThickness_;
		public float farFogZNear_;
		public float farFogZFar_;
		public float farFogHeightDecay_;
		public float farFogDistanceDecay_;
		public float farFogDepthDecay_;
		public float farFogExtinction_;
		public UnityEngine.Color farFogTint_;
		public float farFogStandardHeight_;
		public float farFogBottom_;
		public float farFogPlayerRelative_;
		public float farFogRadius_;
		
        
        new public static void SetupObjectType()
        {
            if (ObjectType != null)
            {
                return;
            }

            var dummy = new VolLightFilterSequenceData();
            var properties = dummy.GetFieldProperties();

            ObjectType = new ObjectType("Black.Sequence.ScreenEffect.VolLightFilterSequenceData", 0, Black.Sequence.ScreenEffect.VolLightFilterSequenceData.ObjectType, Construct, properties, 0, 224);
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

            fieldProperties = new PropertyContainer("Black.Sequence.ScreenEffect.VolLightFilterSequenceData", base.GetFieldProperties(), 149068108, -1426137195);
            
			
			
			fieldProperties.AddProperty(new Property("enabled_", 1722022099, "bool", 8, 1, 1, Property.PrimitiveType.Bool, 0, (char)1));
			fieldProperties.AddProperty(new Property("enableWeather_", 1384777099, "bool", 9, 1, 1, Property.PrimitiveType.Bool, 0, (char)1));
			fieldProperties.AddProperty(new Property("zNear_", 908950600, "float", 12, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("zFar_", 2124336315, "float", 16, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("fogThickness_", 795401080, "float", 20, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("mainlightPower_", 706193422, "float", 24, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("locallightPower_", 1535868686, "float", 28, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("ambientPower_", 3641532409, "float", 32, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("emissivePower_", 3475641920, "float", 36, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("tint_", 4224566825, "Luminous.RenderInterface.Color", 48, 16, 1, Property.PrimitiveType.Color, 0, (char)1));
			fieldProperties.AddProperty(new Property("emissiveColor_", 296452490, "Luminous.RenderInterface.Color", 64, 16, 1, Property.PrimitiveType.Color, 0, (char)1));
			fieldProperties.AddProperty(new Property("ambientCubemapPower_", 203577194, "float", 80, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("ambientCubemapTenPercentHeight_", 4077947168, "float", 84, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("ambientCubemapStartY_", 1255733096, "float", 88, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("ambientDiffuseLightProbe_", 3848827194, "float", 92, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("noiseEnabled_", 1481878015, "bool", 96, 1, 1, Property.PrimitiveType.Bool, 0, (char)1));
			fieldProperties.AddProperty(new Property("noiseAmount_", 2595028454, "float", 100, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("noisePeriod_", 3196379829, "float", 104, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("noiseScrollSpeed_", 2915009852, "float", 108, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("noiseScrollRot_", 3029826630, "float", 112, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("noiseScrollTilt_", 1858906808, "float", 116, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("heightFogEnabled_", 1069938520, "bool", 120, 1, 1, Property.PrimitiveType.Bool, 0, (char)1));
			fieldProperties.AddProperty(new Property("tenPercentHeight_", 3699039887, "float", 124, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("startY_", 2799192663, "float", 128, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("playerRelative_", 152275701, "float", 132, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("fadeInDistance_", 2997034092, "float", 136, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("fadeInToe_", 1941734369, "float", 140, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("fadeInShoulder_", 2033854101, "float", 144, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("blurPassCountVSM_", 1644838455, "int", 148, 4, 1, Property.PrimitiveType.Int32, 0, (char)1));
			fieldProperties.AddProperty(new Property("nearFarContrast_", 1873267867, "float", 152, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("extinction_", 2372565079, "float", 156, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("farFogEnabled_", 1843138292, "bool", 160, 1, 1, Property.PrimitiveType.Bool, 0, (char)1));
			fieldProperties.AddProperty(new Property("farFogThickness_", 3168711629, "float", 164, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("farFogZNear_", 1044922379, "float", 168, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("farFogZFar_", 1138572638, "float", 172, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("farFogHeightDecay_", 1272278942, "float", 176, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("farFogDistanceDecay_", 3224283358, "float", 180, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("farFogDepthDecay_", 3549996168, "float", 184, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("farFogExtinction_", 3879523122, "float", 188, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("farFogTint_", 14930504, "Luminous.RenderInterface.Color", 192, 16, 1, Property.PrimitiveType.Color, 0, (char)1));
			fieldProperties.AddProperty(new Property("farFogStandardHeight_", 3570129557, "float", 208, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("farFogBottom_", 95772886, "float", 212, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("farFogPlayerRelative_", 1295591856, "float", 216, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddProperty(new Property("farFogRadius_", 344166065, "float", 220, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			
			
			return fieldProperties;
        }

		
        private static BaseObject Construct()
        {
            return new VolLightFilterSequenceData();
        }
		
    }
}