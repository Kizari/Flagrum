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

namespace Black.AIGraph.Data.PropertyData
{
    [Serializable, CodeDom.Compiler.GeneratedCode("Luminaire", "0.1")]
    public partial class PropertyMemoryVariableString : Black.AIGraph.Data.PropertyData.PropertyMemoryVariableBase
    {
        new public static ObjectType ObjectType { get; private set; }
        private static PropertyContainer fieldProperties;
		
		public int kind_;
		
        
        new public static void SetupObjectType()
        {
            if (ObjectType != null)
            {
                return;
            }

            var dummy = new PropertyMemoryVariableString();
            var properties = dummy.GetFieldProperties();

            ObjectType = new ObjectType("Black.AIGraph.Data.PropertyData.PropertyMemoryVariableString", 0, Black.AIGraph.Data.PropertyData.PropertyMemoryVariableString.ObjectType, Construct, properties, 0, 40);
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

            fieldProperties = new PropertyContainer("Black.AIGraph.Data.PropertyData.PropertyMemoryVariableString", base.GetFieldProperties(), 634156360, 1587298464);
            
			fieldProperties.AddIndirectlyProperty(new Property("propertyId_", 487541182, "int", 8, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("indexOfLinkedProperty_", 4182715867, "int", 12, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("targetSlotType_", 102733177, "Black.AI.Recog.TARGET_SLOT_TYPE", 24, 4, 1, Property.PrimitiveType.Enum, 0, (char)0));
			
			
			fieldProperties.AddProperty(new Property("kind_", 3595366932, "Black.AI.Recog.TargetMemoryAccessor.ValueKindString", 32, 4, 1, Property.PrimitiveType.Enum, 0, (char)0));
			
			
			return fieldProperties;
        }

		
        private static BaseObject Construct()
        {
            return new PropertyMemoryVariableString();
        }
		
    }
}