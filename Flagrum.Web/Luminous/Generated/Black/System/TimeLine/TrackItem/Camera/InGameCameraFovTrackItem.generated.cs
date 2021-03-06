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

namespace Black.System.TimeLine.TrackItem.Camera
{
    [Serializable, CodeDom.Compiler.GeneratedCode("Luminaire", "0.1")]
    public partial class InGameCameraFovTrackItem : SQEX.Ebony.Framework.TimeControl.TimeLine.TrackItem
    {
        new public static ObjectType ObjectType { get; private set; }
        private static PropertyContainer fieldProperties;
		
		public Black.System.TimeLine.TrackItem.Camera.Struct.InGameCameraFov fovData_= new Black.System.TimeLine.TrackItem.Camera.Struct.InGameCameraFov();
		
        
        new public static void SetupObjectType()
        {
            if (ObjectType != null)
            {
                return;
            }

            var dummy = new InGameCameraFovTrackItem();
            var properties = dummy.GetFieldProperties();

            ObjectType = new ObjectType("Black.System.TimeLine.TrackItem.Camera.InGameCameraFovTrackItem", 0, Black.System.TimeLine.TrackItem.Camera.InGameCameraFovTrackItem.ObjectType, Construct, properties, 0, 216);
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

            fieldProperties = new PropertyContainer("Black.System.TimeLine.TrackItem.Camera.InGameCameraFovTrackItem", base.GetFieldProperties(), -1251062873, 114153324);
            
			fieldProperties.AddIndirectlyProperty(new Property("name_", 182823483, "Ebony.Base.String", 8, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("startTime_", 140908163, "float", 24, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("duration_", 1282328598, "float", 28, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("fovData_.isUse_", 4252696761, "bool", 96, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("fovData_.fovCurveDegree_", 1511594452, "float", 100, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddIndirectlyProperty(new Property("fovData_.fovUseValueType_", 3709996473, "Black.Camera.FovUseValueType", 104, 4, 1, Property.PrimitiveType.Enum, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("fovData_.interpType_", 2625157596, "Black.Camera.RotationInterpType", 108, 4, 1, Property.PrimitiveType.Enum, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("fovData_.timeInterpMode_", 2279307602, "Black.Camera.BlendModeType", 112, 4, 1, Property.PrimitiveType.Enum, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("fovData_.setFovType_", 2031959359, "Black.Camera.SeamlessInGameSetFovType", 116, 4, 1, Property.PrimitiveType.Enum, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("fovData_.interpTime_", 3777021775, "float", 120, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("fovData_.interpVelocity_", 386526241, "float", 124, 4, 1, Property.PrimitiveType.Float, 0, (char)1));
			fieldProperties.AddIndirectlyProperty(new Property("fovData_.autoCompositionOverrideFov_", 3185339512, "float", 128, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("fovData_.isUpdateEveryFrame_", 477637569, "bool", 132, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("fovData_.isKeepFovFromLastSeamlessCamera_", 905858788, "bool", 133, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("fovData_.isIgnoreIncrementFov_", 4109359260, "bool", 134, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("fovData_.useCalculateFov_", 394518596, "Black.System.TimeLine.TrackItem.Camera.Struct.InGameCameraFovNew.CalculateFOVType", 136, 4, 1, Property.PrimitiveType.Enum, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("fovData_.maxTargetRaidus_", 3540973189, "float", 140, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("fovData_.minTargetRaidus_", 1968280059, "float", 144, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("fovData_.boundingSizeRate_", 1937741635, "float", 148, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("fovData_.startData_", 424214896, "Black.System.TimeLine.TrackItem.Camera.Struct.InGameCameraFovStart", 160, 32, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("fovData_.startData_.isChangeStartFov_", 749153606, "bool", 168, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("fovData_.startData_.fovStartInterpMode_", 855981111, "Black.Camera.BlendModeType", 172, 4, 1, Property.PrimitiveType.Enum, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("fovData_.startData_.fovStartInterpTime_", 887310945, "float", 176, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("fovData_.startData_.fovStartFInterpToSpeed_", 1320334068, "float", 180, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("fovData_.startData_.fovStartDegree_", 577660132, "float", 184, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("fovData_.endData_", 1130584631, "Black.System.TimeLine.TrackItem.Camera.Struct.InGameCameraFovEnd", 192, 24, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("fovData_.endData_.isChangeEndFov_", 1745134364, "bool", 200, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("fovData_.endData_.fovEndInterpMode_", 4188832351, "Black.Camera.BlendModeType", 204, 4, 1, Property.PrimitiveType.Enum, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("fovData_.endData_.fovEndInterpTime_", 379396361, "float", 208, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("fovData_.endData_.fovEndFInterpToSpeed_", 574931980, "float", 212, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			
			
			fieldProperties.AddProperty(new Property("fovData_", 3792695137, "Black.System.TimeLine.TrackItem.Camera.Struct.InGameCameraFov", 88, 128, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			
			
			return fieldProperties;
        }

		
        private static BaseObject Construct()
        {
            return new InGameCameraFovTrackItem();
        }
		
    }
}