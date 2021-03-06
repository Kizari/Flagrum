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

namespace Black.Sequence.Action.MiniGame.Colosseum
{
    [Serializable, CodeDom.Compiler.GeneratedCode("Luminaire", "0.1")]
    public partial class SequenceActionColosseumSendSituation : SQEX.Ebony.Framework.Sequence.SequenceAction
    {
        new public static ObjectType ObjectType { get; private set; }
        private static PropertyContainer fieldProperties;
		
		public SQEX.Ebony.Framework.Node.GraphTriggerInputPin startPin_= new SQEX.Ebony.Framework.Node.GraphTriggerInputPin();
		public SQEX.Ebony.Framework.Node.GraphTriggerInputPin stopPin_= new SQEX.Ebony.Framework.Node.GraphTriggerInputPin();
		public SQEX.Ebony.Framework.Node.GraphTriggerInputPin updatePin_= new SQEX.Ebony.Framework.Node.GraphTriggerInputPin();
		public SQEX.Ebony.Framework.Node.GraphVariableInputPin waitTimePin_= new SQEX.Ebony.Framework.Node.GraphVariableInputPin();
		public SQEX.Ebony.Framework.Node.GraphVariableInputPin overWriteWaitTimePin_= new SQEX.Ebony.Framework.Node.GraphVariableInputPin();
		public SQEX.Ebony.Framework.Node.GraphTriggerOutputPin dynamicPin_= new SQEX.Ebony.Framework.Node.GraphTriggerOutputPin();
		public SQEX.Ebony.Framework.Node.GraphTriggerOutputPin diePin_= new SQEX.Ebony.Framework.Node.GraphTriggerOutputPin();
		public SQEX.Ebony.Framework.Node.GraphTriggerOutputPin randomPin_= new SQEX.Ebony.Framework.Node.GraphTriggerOutputPin();
		public SQEX.Ebony.Framework.Node.GraphTriggerOutputPin finishedPin_= new SQEX.Ebony.Framework.Node.GraphTriggerOutputPin();
		public SQEX.Ebony.Framework.Node.GraphVariableOutputPin actorPin_= new SQEX.Ebony.Framework.Node.GraphVariableOutputPin();
		public SQEX.Ebony.Framework.Node.GraphVariableOutputPin positionPin_= new SQEX.Ebony.Framework.Node.GraphVariableOutputPin();
		public SQEX.Ebony.Framework.Node.GraphVariableOutputPin targetActorPin_= new SQEX.Ebony.Framework.Node.GraphVariableOutputPin();
		public SQEX.Ebony.Framework.Node.GraphVariableOutputPin targetPositionPin_= new SQEX.Ebony.Framework.Node.GraphVariableOutputPin();
		public SQEX.Ebony.Framework.Node.GraphVariableOutputPin countPin_= new SQEX.Ebony.Framework.Node.GraphVariableOutputPin();
		public SQEX.Ebony.Framework.Node.GraphVariableOutputPin isBetGroupPin_= new SQEX.Ebony.Framework.Node.GraphVariableOutputPin();
		
        
        new public static void SetupObjectType()
        {
            if (ObjectType != null)
            {
                return;
            }

            var dummy = new SequenceActionColosseumSendSituation();
            var properties = dummy.GetFieldProperties();

            ObjectType = new ObjectType("Black.Sequence.Action.MiniGame.Colosseum.SequenceActionColosseumSendSituation", 0, Black.Sequence.Action.MiniGame.Colosseum.SequenceActionColosseumSendSituation.ObjectType, Construct, properties, 0, 1648);
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

            fieldProperties = new PropertyContainer("Black.Sequence.Action.MiniGame.Colosseum.SequenceActionColosseumSendSituation", base.GetFieldProperties(), 359838932, 544900440);
            
			fieldProperties.AddIndirectlyProperty(new Property("refInPorts_", 1035088696, "SQEX.Ebony.Std.DynamicArray< SQEX.Ebony.Framework.Node.GraphPin*, MEMORY_CATEGORY_FRAMEWORK >", 24, 16, 1, Property.PrimitiveType.PointerArray, 0, (char)4));
			fieldProperties.AddIndirectlyProperty(new Property("refOutPorts_", 283683627, "SQEX.Ebony.Std.DynamicArray< SQEX.Ebony.Framework.Node.GraphPin*, MEMORY_CATEGORY_FRAMEWORK >", 40, 16, 1, Property.PrimitiveType.PointerArray, 0, (char)4));
			fieldProperties.AddIndirectlyProperty(new Property("triInPorts_", 291734708, "SQEX.Ebony.Std.DynamicArray< SQEX.Ebony.Framework.Node.GraphPin*, MEMORY_CATEGORY_FRAMEWORK >", 96, 16, 1, Property.PrimitiveType.PointerArray, 0, (char)4));
			fieldProperties.AddIndirectlyProperty(new Property("triOutPorts_", 3107891487, "SQEX.Ebony.Std.DynamicArray< SQEX.Ebony.Framework.Node.GraphPin*, MEMORY_CATEGORY_FRAMEWORK >", 112, 16, 1, Property.PrimitiveType.PointerArray, 0, (char)4));
			fieldProperties.AddIndirectlyProperty(new Property("Isolated_", 56305607, "bool", 168, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("startPin_.pinName_", 3393751070, "Base.String", 184, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("startPin_.name_", 2709444057, "Base.String", 200, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("startPin_.connections_", 3972754729, "SQEX.Ebony.Std.DynamicArray< SQEX.Ebony.Framework.Node.GraphPin* >", 216, 16, 1, Property.PrimitiveType.PointerArray, 0, (char)1));
			fieldProperties.AddIndirectlyProperty(new Property("startPin_.delayType_", 2098578323, "SQEX.Ebony.Framework.Node.GraphPin.DelayType", 248, 4, 1, Property.PrimitiveType.Enum, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("startPin_.delayTime_", 2204508536, "float", 252, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("startPin_.delayMaxTime_", 3635708722, "float", 256, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("stopPin_.pinName_", 2791583880, "Base.String", 280, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("stopPin_.name_", 876131683, "Base.String", 296, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("stopPin_.connections_", 82978923, "SQEX.Ebony.Std.DynamicArray< SQEX.Ebony.Framework.Node.GraphPin* >", 312, 16, 1, Property.PrimitiveType.PointerArray, 0, (char)1));
			fieldProperties.AddIndirectlyProperty(new Property("stopPin_.delayType_", 4178339557, "SQEX.Ebony.Framework.Node.GraphPin.DelayType", 344, 4, 1, Property.PrimitiveType.Enum, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("stopPin_.delayTime_", 885190114, "float", 348, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("stopPin_.delayMaxTime_", 162024376, "float", 352, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("updatePin_.pinName_", 2651424603, "Base.String", 376, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("updatePin_.name_", 838726806, "Base.String", 392, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("updatePin_.connections_", 423420588, "SQEX.Ebony.Std.DynamicArray< SQEX.Ebony.Framework.Node.GraphPin* >", 408, 16, 1, Property.PrimitiveType.PointerArray, 0, (char)1));
			fieldProperties.AddIndirectlyProperty(new Property("updatePin_.delayType_", 3998978274, "SQEX.Ebony.Framework.Node.GraphPin.DelayType", 440, 4, 1, Property.PrimitiveType.Enum, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("updatePin_.delayTime_", 3494297189, "float", 444, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("updatePin_.delayMaxTime_", 3721925441, "float", 448, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("waitTimePin_.pinName_", 899520436, "Base.String", 472, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("waitTimePin_.name_", 3399170767, "Base.String", 488, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("waitTimePin_.connections_", 3641837407, "SQEX.Ebony.Std.DynamicArray< SQEX.Ebony.Framework.Node.GraphPin* >", 504, 16, 1, Property.PrimitiveType.PointerArray, 0, (char)1));
			fieldProperties.AddIndirectlyProperty(new Property("waitTimePin_.pinValueType_", 1112753874, "Base.String", 536, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("overWriteWaitTimePin_.pinName_", 3637536049, "Base.String", 560, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("overWriteWaitTimePin_.name_", 1084066348, "Base.String", 576, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("overWriteWaitTimePin_.connections_", 1657037746, "SQEX.Ebony.Std.DynamicArray< SQEX.Ebony.Framework.Node.GraphPin* >", 592, 16, 1, Property.PrimitiveType.PointerArray, 0, (char)1));
			fieldProperties.AddIndirectlyProperty(new Property("overWriteWaitTimePin_.pinValueType_", 1600596737, "Base.String", 624, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("dynamicPin_.pinName_", 699185897, "Base.String", 648, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("dynamicPin_.name_", 3724757812, "Base.String", 664, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("dynamicPin_.connections_", 3560346250, "SQEX.Ebony.Std.DynamicArray< SQEX.Ebony.Framework.Node.GraphPin* >", 680, 16, 1, Property.PrimitiveType.PointerArray, 0, (char)1));
			fieldProperties.AddIndirectlyProperty(new Property("dynamicPin_.delayType_", 2654770744, "SQEX.Ebony.Framework.Node.GraphPin.DelayType", 712, 4, 1, Property.PrimitiveType.Enum, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("dynamicPin_.delayTime_", 1503220851, "float", 716, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("dynamicPin_.delayMaxTime_", 1761040427, "float", 720, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("diePin_.pinName_", 2461006348, "Base.String", 744, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("diePin_.name_", 3250243767, "Base.String", 760, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("diePin_.connections_", 1751271575, "SQEX.Ebony.Std.DynamicArray< SQEX.Ebony.Framework.Node.GraphPin* >", 776, 16, 1, Property.PrimitiveType.PointerArray, 0, (char)1));
			fieldProperties.AddIndirectlyProperty(new Property("diePin_.delayType_", 3738067409, "SQEX.Ebony.Framework.Node.GraphPin.DelayType", 808, 4, 1, Property.PrimitiveType.Enum, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("diePin_.delayTime_", 2963742598, "float", 812, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("diePin_.delayMaxTime_", 3034786476, "float", 816, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("randomPin_.pinName_", 234570997, "Base.String", 840, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("randomPin_.name_", 3368883248, "Base.String", 856, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("randomPin_.connections_", 1645229182, "SQEX.Ebony.Std.DynamicArray< SQEX.Ebony.Framework.Node.GraphPin* >", 872, 16, 1, Property.PrimitiveType.PointerArray, 0, (char)1));
			fieldProperties.AddIndirectlyProperty(new Property("randomPin_.delayType_", 1261601132, "SQEX.Ebony.Framework.Node.GraphPin.DelayType", 904, 4, 1, Property.PrimitiveType.Enum, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("randomPin_.delayTime_", 3329150239, "float", 908, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("randomPin_.delayMaxTime_", 773284199, "float", 912, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("finishedPin_.pinName_", 1531062106, "Base.String", 936, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("finishedPin_.name_", 1598469413, "Base.String", 952, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("finishedPin_.connections_", 1442084285, "SQEX.Ebony.Std.DynamicArray< SQEX.Ebony.Framework.Node.GraphPin* >", 968, 16, 1, Property.PrimitiveType.PointerArray, 0, (char)1));
			fieldProperties.AddIndirectlyProperty(new Property("finishedPin_.delayType_", 2727021751, "SQEX.Ebony.Framework.Node.GraphPin.DelayType", 1000, 4, 1, Property.PrimitiveType.Enum, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("finishedPin_.delayTime_", 2883353844, "float", 1004, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("finishedPin_.delayMaxTime_", 276701966, "float", 1008, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("actorPin_.pinName_", 61679437, "Base.String", 1032, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("actorPin_.name_", 700809432, "Base.String", 1048, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("actorPin_.connections_", 2962364278, "SQEX.Ebony.Std.DynamicArray< SQEX.Ebony.Framework.Node.GraphPin* >", 1064, 16, 1, Property.PrimitiveType.PointerArray, 0, (char)1));
			fieldProperties.AddIndirectlyProperty(new Property("actorPin_.pinValueType_", 2807496477, "Base.String", 1096, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("positionPin_.pinName_", 1412606077, "Base.String", 1120, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("positionPin_.name_", 1819507336, "Base.String", 1136, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("positionPin_.connections_", 1679432646, "SQEX.Ebony.Std.DynamicArray< SQEX.Ebony.Framework.Node.GraphPin* >", 1152, 16, 1, Property.PrimitiveType.PointerArray, 0, (char)1));
			fieldProperties.AddIndirectlyProperty(new Property("positionPin_.pinValueType_", 3759149005, "Base.String", 1184, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("targetActorPin_.pinName_", 1051229172, "Base.String", 1208, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("targetActorPin_.name_", 188225807, "Base.String", 1224, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("targetActorPin_.connections_", 2518678943, "SQEX.Ebony.Std.DynamicArray< SQEX.Ebony.Framework.Node.GraphPin* >", 1240, 16, 1, Property.PrimitiveType.PointerArray, 0, (char)1));
			fieldProperties.AddIndirectlyProperty(new Property("targetActorPin_.pinValueType_", 2604514834, "Base.String", 1272, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("targetPositionPin_.pinName_", 3443077106, "Base.String", 1296, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("targetPositionPin_.name_", 1722637997, "Base.String", 1312, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("targetPositionPin_.connections_", 820637333, "SQEX.Ebony.Std.DynamicArray< SQEX.Ebony.Framework.Node.GraphPin* >", 1328, 16, 1, Property.PrimitiveType.PointerArray, 0, (char)1));
			fieldProperties.AddIndirectlyProperty(new Property("targetPositionPin_.pinValueType_", 3745212692, "Base.String", 1360, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("countPin_.pinName_", 3427603419, "Base.String", 1384, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("countPin_.name_", 3605285398, "Base.String", 1400, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("countPin_.connections_", 4048336684, "SQEX.Ebony.Std.DynamicArray< SQEX.Ebony.Framework.Node.GraphPin* >", 1416, 16, 1, Property.PrimitiveType.PointerArray, 0, (char)1));
			fieldProperties.AddIndirectlyProperty(new Property("countPin_.pinValueType_", 383713647, "Base.String", 1448, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("isBetGroupPin_.pinName_", 3218886654, "Base.String", 1472, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("isBetGroupPin_.name_", 1299314425, "Base.String", 1488, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("isBetGroupPin_.connections_", 1614454345, "SQEX.Ebony.Std.DynamicArray< SQEX.Ebony.Framework.Node.GraphPin* >", 1504, 16, 1, Property.PrimitiveType.PointerArray, 0, (char)1));
			fieldProperties.AddIndirectlyProperty(new Property("isBetGroupPin_.pinValueType_", 578933424, "Base.String", 1536, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			
			
			fieldProperties.AddProperty(new Property("startPin_", 2664043275, "SQEX.Ebony.Framework.Node.GraphTriggerInputPin", 176, 96, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("stopPin_", 2226130577, "SQEX.Ebony.Framework.Node.GraphTriggerInputPin", 272, 96, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("updatePin_", 1593847028, "SQEX.Ebony.Framework.Node.GraphTriggerInputPin", 368, 96, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("waitTimePin_", 4104434725, "SQEX.Ebony.Framework.Node.GraphVariableInputPin", 464, 88, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("overWriteWaitTimePin_", 2762273062, "SQEX.Ebony.Framework.Node.GraphVariableInputPin", 552, 88, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("dynamicPin_", 517124814, "SQEX.Ebony.Framework.Node.GraphTriggerOutputPin", 640, 96, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("diePin_", 2858396653, "SQEX.Ebony.Framework.Node.GraphTriggerOutputPin", 736, 96, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("randomPin_", 178221298, "SQEX.Ebony.Framework.Node.GraphTriggerOutputPin", 832, 96, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("finishedPin_", 1775979791, "SQEX.Ebony.Framework.Node.GraphTriggerOutputPin", 928, 96, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("actorPin_", 3381458010, "SQEX.Ebony.Framework.Node.GraphVariableOutputPin", 1024, 88, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("positionPin_", 1766575530, "SQEX.Ebony.Framework.Node.GraphVariableOutputPin", 1112, 88, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("targetActorPin_", 422699493, "SQEX.Ebony.Framework.Node.GraphVariableOutputPin", 1200, 88, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("targetPositionPin_", 3520819287, "SQEX.Ebony.Framework.Node.GraphVariableOutputPin", 1288, 88, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("countPin_", 484281972, "SQEX.Ebony.Framework.Node.GraphVariableOutputPin", 1376, 88, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("isBetGroupPin_", 3816746091, "SQEX.Ebony.Framework.Node.GraphVariableOutputPin", 1464, 88, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			
			
			return fieldProperties;
        }

		
        private static BaseObject Construct()
        {
            return new SequenceActionColosseumSendSituation();
        }
		
    }
}