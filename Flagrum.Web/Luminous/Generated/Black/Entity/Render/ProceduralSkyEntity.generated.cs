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

namespace Black.Entity.Render
{
    [Serializable, CodeDom.Compiler.GeneratedCode("Luminaire", "0.1")]
    public partial class ProceduralSkyEntity : SQEX.Ebony.Framework.Entity.Entity
    {
        new public static ObjectType ObjectType { get; private set; }
        private static PropertyContainer fieldProperties;
		
		public bool enableSky_;
		public bool enableObjects_;
		public bool enableCloud_;
		public bool enableSkyCubemap_;
		public bool enableFilteredSpecularCubemap_;
		public bool enableBillboardStar_;
		public float starTilingFrequency0_;
		public float starTilingFrequency1_;
		public float starTilingFrequency2_;
		public float starTilingFrequency3_;
		public float starTwinkleFrequency0_;
		public float starTwinkleFrequency1_;
		public bool enableMilkyWay_;
		public float milkywayFrequency_;
		public float milkywayOffset_;
		public float milkyWayPhi0_;
		public float milkyWayPhi1_;
		public float milkywayRotX_;
		public int milkywayResolution_;
		public bool enableMoon_;
		public string moonPath_= string.Empty;
		public string tilingStarPath_= string.Empty;
		public string tilingStarPath2_= string.Empty;
		public string starMaskPath_= string.Empty;
		public string milkyWayPath_= string.Empty;
		public string starTwinklePath_= string.Empty;
		public string highCloudPath_= string.Empty;
		public string billboardStarPath_= string.Empty;
		public string hc0Path_= string.Empty;
		public string hc1Path_= string.Empty;
		public float shadowStretch_;
		public float starBrightnessThreshold_;
		public float hcHeight_;
		public float hcHeightOffset_;
		public float hcOpacity_;
		public float hcRotSpeed_;
		public float hcHorOffset_;
		public float hcTiling_;
		public float hcBlend_;
		public float hcBlendSpeed_;
		public float hcAniso_;
		
        
        new public static void SetupObjectType()
        {
            if (ObjectType != null)
            {
                return;
            }

            var dummy = new ProceduralSkyEntity();
            var properties = dummy.GetFieldProperties();

            ObjectType = new ObjectType("Black.Entity.Render.ProceduralSkyEntity", 0, Black.Entity.Render.ProceduralSkyEntity.ObjectType, Construct, properties, 0, 688);
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

            fieldProperties = new PropertyContainer("Black.Entity.Render.ProceduralSkyEntity", base.GetFieldProperties(), 503171251, -1505870931);
            
			
			
			fieldProperties.AddProperty(new Property("enableSky_", 882296488, "bool", 64, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
			fieldProperties.AddProperty(new Property("enableObjects_", 90900479, "bool", 65, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
			fieldProperties.AddProperty(new Property("enableCloud_", 3609682428, "bool", 66, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
			fieldProperties.AddProperty(new Property("enableSkyCubemap_", 783708897, "bool", 67, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
			fieldProperties.AddProperty(new Property("enableFilteredSpecularCubemap_", 2405456278, "bool", 68, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
			fieldProperties.AddProperty(new Property("enableBillboardStar_", 2319801722, "bool", 69, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
			fieldProperties.AddProperty(new Property("starTilingFrequency0_", 508197637, "float", 72, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddProperty(new Property("starTilingFrequency1_", 1582112348, "float", 76, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddProperty(new Property("starTilingFrequency2_", 1582259443, "float", 80, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddProperty(new Property("starTilingFrequency3_", 508742090, "float", 84, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddProperty(new Property("starTwinkleFrequency0_", 1760705558, "float", 88, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddProperty(new Property("starTwinkleFrequency1_", 2834222911, "float", 92, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddProperty(new Property("enableMilkyWay_", 2437878940, "bool", 96, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
			fieldProperties.AddProperty(new Property("milkywayFrequency_", 497552851, "float", 100, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddProperty(new Property("milkywayOffset_", 4250572020, "float", 104, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddProperty(new Property("milkyWayPhi0_", 2339545938, "float", 108, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddProperty(new Property("milkyWayPhi1_", 2339398843, "float", 112, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddProperty(new Property("milkywayRotX_", 1595074016, "float", 116, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddProperty(new Property("milkywayResolution_", 1242612391, "unsigned int", 120, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddProperty(new Property("enableMoon_", 2374959692, "bool", 124, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
			fieldProperties.AddProperty(new Property("moonPath_", 1204382472, "Ebony.Base.String", 128, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddProperty(new Property("tilingStarPath_", 369050458, "Ebony.Base.String", 144, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddProperty(new Property("tilingStarPath2_", 3838676098, "Ebony.Base.String", 160, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddProperty(new Property("starMaskPath_", 3816175139, "Ebony.Base.String", 176, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddProperty(new Property("milkyWayPath_", 2987047368, "Ebony.Base.String", 192, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddProperty(new Property("starTwinklePath_", 4158609293, "Ebony.Base.String", 208, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddProperty(new Property("highCloudPath_", 1446996438, "Ebony.Base.String", 224, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddProperty(new Property("billboardStarPath_", 1277059548, "Ebony.Base.String", 240, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddProperty(new Property("hc0Path_", 112896036, "Ebony.Base.String", 256, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddProperty(new Property("hc1Path_", 2650084717, "Ebony.Base.String", 272, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddProperty(new Property("shadowStretch_", 2091290987, "float", 288, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddProperty(new Property("starBrightnessThreshold_", 1434265498, "float", 292, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddProperty(new Property("hcHeight_", 3792642816, "float", 296, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddProperty(new Property("hcHeightOffset_", 3251812993, "float", 300, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddProperty(new Property("hcOpacity_", 4216307778, "float", 304, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddProperty(new Property("hcRotSpeed_", 472444329, "float", 308, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddProperty(new Property("hcHorOffset_", 2599976627, "float", 312, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddProperty(new Property("hcTiling_", 2135929364, "float", 316, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddProperty(new Property("hcBlend_", 2027686000, "float", 320, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddProperty(new Property("hcBlendSpeed_", 718795311, "float", 324, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddProperty(new Property("hcAniso_", 2330243373, "float", 328, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			
			
			return fieldProperties;
        }

		
        private static BaseObject Construct()
        {
            return new ProceduralSkyEntity();
        }
		
    }
}