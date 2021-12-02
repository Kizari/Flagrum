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

namespace Black.AIGraph.Extend.Tray
{
    [Serializable, CodeDom.Compiler.GeneratedCode("Luminaire", "0.1")]
    public partial class AIGraphTrayMindTaskFSM : SQEX.Ebony.AIGraph.Tray.AIGraphTrayFSM
    {
        new public static ObjectType ObjectType { get; private set; }
        private static PropertyContainer fieldProperties;
		
		public SQEX.Ebony.AIGraph.Data.PropertyData.PropertyFixid id= new SQEX.Ebony.AIGraph.Data.PropertyData.PropertyFixid();
		public SQEX.Ebony.AIGraph.Data.PropertyData.PropertyInt priority= new SQEX.Ebony.AIGraph.Data.PropertyData.PropertyInt();
		public SQEX.Ebony.AIGraph.Data.PropertyData.PropertyFloat timeoutTime= new SQEX.Ebony.AIGraph.Data.PropertyData.PropertyFloat();
		public SQEX.Ebony.AIGraph.Data.PropertyData.PropertyFloat waitTime= new SQEX.Ebony.AIGraph.Data.PropertyData.PropertyFloat();
		public SQEX.Ebony.AIGraph.Data.PropertyData.PropertyBool isEnableWait= new SQEX.Ebony.AIGraph.Data.PropertyData.PropertyBool();
		public SQEX.Ebony.AIGraph.Data.PropertyData.PropertyBool bResetMindLayer= new SQEX.Ebony.AIGraph.Data.PropertyData.PropertyBool();
		public SQEX.Ebony.AIGraph.Data.PropertyData.PropertyBool bSuspendMindLayer= new SQEX.Ebony.AIGraph.Data.PropertyData.PropertyBool();
		
        
        new public static void SetupObjectType()
        {
            if (ObjectType != null)
            {
                return;
            }

            var dummy = new AIGraphTrayMindTaskFSM();
            var properties = dummy.GetFieldProperties();

            ObjectType = new ObjectType("Black.AIGraph.Extend.Tray.AIGraphTrayMindTaskFSM", 0, Black.AIGraph.Extend.Tray.AIGraphTrayMindTaskFSM.ObjectType, Construct, properties, 0, 440);
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

            fieldProperties = new PropertyContainer("Black.AIGraph.Extend.Tray.AIGraphTrayMindTaskFSM", base.GetFieldProperties(), 1346227240, 1446934624);
            
			fieldProperties.AddIndirectlyProperty(new Property("uid_", 2695886806, "int", 16, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("startNodeUid_", 2715036948, "int", 20, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("bDisable_", 54874740, "bool", 24, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("bSkipblackBoardInitialization_", 1945287384, "bool", 25, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("nodes_", 61650911, "SQEX.Ebony.Std.DynamicArray< SQEX.Ebony.AIGraph.Node.AIGraphNodeBase*, MEMORY_CATEGORY_AI_GRAPH >", 48, 16, 1, Property.PrimitiveType.PointerArray, 0, (char)4));
			fieldProperties.AddIndirectlyProperty(new Property("properties_", 2753876537, "SQEX.Ebony.Std.DynamicArray< SQEX.Ebony.AIGraph.Data.PropertyData.PropertyBase*, MEMORY_CATEGORY_AI_GRAPH >", 64, 16, 1, Property.PrimitiveType.PointerArray, 0, (char)4));
			fieldProperties.AddIndirectlyProperty(new Property("debug_BlockWarningRunningWithNoChildren_", 2490290986, "SQEX.Ebony.AIGraph.Data.PropertyData.PropertyBool", 80, 32, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("debug_BlockWarningRunningWithNoChildren_.propertyId_", 1359588821, "int", 88, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("debug_BlockWarningRunningWithNoChildren_.indexOfLinkedProperty_", 1505766990, "int", 92, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("debug_BlockWarningRunningWithNoChildren_.value_", 1075728398, "bool", 104, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("transits_", 3559974222, "SQEX.Ebony.Std.DynamicArray< SQEX.Ebony.AIGraph.Data.TransitionData*, MEMORY_CATEGORY_AI_GRAPH >", 160, 16, 1, Property.PrimitiveType.PointerArray, 0, (char)2));
			fieldProperties.AddIndirectlyProperty(new Property("rootNodes_", 509121599, "SQEX.Ebony.Std.DynamicArray< Ebony.AIGraph.Node.FSM.AIGraphNodeFSMRoot*, MEMORY_CATEGORY_AI_GRAPH >", 176, 16, 1, Property.PrimitiveType.PointerArray, 0, (char)2));
			fieldProperties.AddIndirectlyProperty(new Property("id.propertyId_", 3123770551, "int", 216, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("id.indexOfLinkedProperty_", 3124156648, "int", 220, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("id.value_", 2062651460, "SQEX.Ebony.Std.Fixid", 232, 4, 1, Property.PrimitiveType.Fixid, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("priority.propertyId_", 697600094, "int", 248, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("priority.indexOfLinkedProperty_", 2209435387, "int", 252, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("priority.value_", 890667759, "int", 264, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("timeoutTime.propertyId_", 380664998, "int", 280, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("timeoutTime.indexOfLinkedProperty_", 935367299, "int", 284, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("timeoutTime.value_", 1264133255, "float", 296, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("waitTime.propertyId_", 3410028590, "int", 312, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("waitTime.indexOfLinkedProperty_", 409995979, "int", 316, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("waitTime.value_", 3042139359, "float", 328, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("isEnableWait.propertyId_", 1606593212, "int", 344, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("isEnableWait.indexOfLinkedProperty_", 1907814985, "int", 348, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("isEnableWait.value_", 2449717145, "bool", 360, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("bResetMindLayer.propertyId_", 3833800322, "int", 376, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("bResetMindLayer.indexOfLinkedProperty_", 998544567, "int", 380, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("bResetMindLayer.value_", 3208589163, "bool", 392, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("bSuspendMindLayer.propertyId_", 3367623923, "int", 408, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("bSuspendMindLayer.indexOfLinkedProperty_", 1747582812, "int", 412, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("bSuspendMindLayer.value_", 2960772360, "bool", 424, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
			
			
			fieldProperties.AddProperty(new Property("id", 926444256, "SQEX.Ebony.AIGraph.Data.PropertyData.PropertyFixid", 208, 32, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("priority", 2498028297, "SQEX.Ebony.AIGraph.Data.PropertyData.PropertyInt", 240, 32, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("timeoutTime", 350021985, "SQEX.Ebony.AIGraph.Data.PropertyData.PropertyFloat", 272, 32, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("waitTime", 1213593401, "SQEX.Ebony.AIGraph.Data.PropertyData.PropertyFloat", 304, 32, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("isEnableWait", 2233842355, "SQEX.Ebony.AIGraph.Data.PropertyData.PropertyBool", 336, 32, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("bResetMindLayer", 772950909, "SQEX.Ebony.AIGraph.Data.PropertyData.PropertyBool", 368, 32, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("bSuspendMindLayer", 310129772, "SQEX.Ebony.AIGraph.Data.PropertyData.PropertyBool", 400, 32, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			
			
			return fieldProperties;
        }

		
        private static BaseObject Construct()
        {
            return new AIGraphTrayMindTaskFSM();
        }
		
    }
}