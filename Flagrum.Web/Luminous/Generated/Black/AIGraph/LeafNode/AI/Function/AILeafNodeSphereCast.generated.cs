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

namespace Black.AIGraph.LeafNode.AI.Function
{
    [Serializable, CodeDom.Compiler.GeneratedCode("Luminaire", "0.1")]
    public partial class AILeafNodeSphereCast : SQEX.Ebony.AIGraph.Node.Leaf.AIGraphNodeLeafBase
    {
        new public static ObjectType ObjectType { get; private set; }
        private static PropertyContainer fieldProperties;
		
		public SQEX.Ebony.AIGraph.Data.PropertyData.PropertyVector startPosition_= new SQEX.Ebony.AIGraph.Data.PropertyData.PropertyVector();
		public SQEX.Ebony.AIGraph.Data.PropertyData.PropertyVector startOffset_= new SQEX.Ebony.AIGraph.Data.PropertyData.PropertyVector();
		public SQEX.Ebony.AIGraph.Data.PropertyData.PropertyVector endPosition_= new SQEX.Ebony.AIGraph.Data.PropertyData.PropertyVector();
		public SQEX.Ebony.AIGraph.Data.PropertyData.PropertyVector endOffset_= new SQEX.Ebony.AIGraph.Data.PropertyData.PropertyVector();
		public SQEX.Ebony.AIGraph.Data.PropertyData.PropertyVector direction_= new SQEX.Ebony.AIGraph.Data.PropertyData.PropertyVector();
		public SQEX.Ebony.AIGraph.Data.PropertyData.PropertyFloat distance_= new SQEX.Ebony.AIGraph.Data.PropertyData.PropertyFloat();
		public SQEX.Ebony.AIGraph.Data.PropertyData.PropertyFloat sphereRadius_= new SQEX.Ebony.AIGraph.Data.PropertyData.PropertyFloat();
		public SQEX.Ebony.AIGraph.Data.PropertyData.PropertyBool hasHit_= new SQEX.Ebony.AIGraph.Data.PropertyData.PropertyBool();
		public SQEX.Ebony.AIGraph.Data.PropertyData.PropertyVector hitPosition_= new SQEX.Ebony.AIGraph.Data.PropertyData.PropertyVector();
		public SQEX.Ebony.AIGraph.Data.PropertyData.PropertyFloat hitDistance_= new SQEX.Ebony.AIGraph.Data.PropertyData.PropertyFloat();
		public SQEX.Ebony.AIGraph.Data.PropertyData.PropertyFloat timeout_= new SQEX.Ebony.AIGraph.Data.PropertyData.PropertyFloat();
		
        
        new public static void SetupObjectType()
        {
            if (ObjectType != null)
            {
                return;
            }

            var dummy = new AILeafNodeSphereCast();
            var properties = dummy.GetFieldProperties();

            ObjectType = new ObjectType("Black.AIGraph.LeafNode.AI.Function.AILeafNodeSphereCast", 0, Black.AIGraph.LeafNode.AI.Function.AILeafNodeSphereCast.ObjectType, Construct, properties, 0, 624);
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

            fieldProperties = new PropertyContainer("Black.AIGraph.LeafNode.AI.Function.AILeafNodeSphereCast", base.GetFieldProperties(), -150567363, -158273098);
            
			fieldProperties.AddIndirectlyProperty(new Property("uid_", 2695886806, "int", 16, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("startNodeUid_", 2715036948, "int", 20, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("bDisable_", 54874740, "bool", 24, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("bSkipblackBoardInitialization_", 1945287384, "bool", 25, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("startPosition_.propertyId_", 3906347278, "int", 72, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("startPosition_.indexOfLinkedProperty_", 1912591019, "int", 76, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("startPosition_.value_", 3784575999, "Luminous.Math.VectorA", 96, 16, 1, Property.PrimitiveType.Vector4, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("startOffset_.propertyId_", 2102518598, "int", 136, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("startOffset_.indexOfLinkedProperty_", 3160136611, "int", 140, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("startOffset_.value_", 2850180967, "Luminous.Math.VectorA", 160, 16, 1, Property.PrimitiveType.Vector4, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("endPosition_.propertyId_", 4262907289, "int", 200, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("endPosition_.indexOfLinkedProperty_", 3760715562, "int", 204, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("endPosition_.value_", 3944473498, "Luminous.Math.VectorA", 224, 16, 1, Property.PrimitiveType.Vector4, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("endOffset_.propertyId_", 572049489, "int", 264, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("endOffset_.indexOfLinkedProperty_", 3443811298, "int", 268, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("endOffset_.value_", 1614968066, "Luminous.Math.VectorA", 288, 16, 1, Property.PrimitiveType.Vector4, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("direction_.propertyId_", 3541951416, "int", 328, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("direction_.indexOfLinkedProperty_", 841039981, "int", 332, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("direction_.value_", 169957693, "Luminous.Math.VectorA", 352, 16, 1, Property.PrimitiveType.Vector4, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("distance_.propertyId_", 287175168, "int", 392, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("distance_.indexOfLinkedProperty_", 2625758549, "int", 396, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("distance_.value_", 1949025525, "float", 408, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("sphereRadius_.propertyId_", 1713575788, "int", 424, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("sphereRadius_.indexOfLinkedProperty_", 190797305, "int", 428, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("sphereRadius_.value_", 1939202729, "float", 440, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("hasHit_.propertyId_", 2846786838, "int", 456, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("hasHit_.indexOfLinkedProperty_", 2300003827, "int", 460, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("hasHit_.value_", 2171922583, "bool", 472, 1, 1, Property.PrimitiveType.Bool, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("hitPosition_.propertyId_", 3795785369, "int", 488, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("hitPosition_.indexOfLinkedProperty_", 1674164778, "int", 492, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("hitPosition_.value_", 259450522, "Luminous.Math.VectorA", 512, 16, 1, Property.PrimitiveType.Vector4, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("hitDistance_.propertyId_", 1414804501, "int", 552, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("hitDistance_.indexOfLinkedProperty_", 2384080014, "int", 556, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("hitDistance_.value_", 470088526, "float", 568, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("timeout_.propertyId_", 476365610, "int", 584, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("timeout_.indexOfLinkedProperty_", 2241916255, "int", 588, 4, 1, Property.PrimitiveType.Int32, 0, (char)0));
			fieldProperties.AddIndirectlyProperty(new Property("timeout_.value_", 809447011, "float", 600, 4, 1, Property.PrimitiveType.Float, 0, (char)0));
			
			
			fieldProperties.AddProperty(new Property("startPosition_", 3770512409, "SQEX.Ebony.AIGraph.Data.PropertyData.PropertyVector", 64, 64, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("startOffset_", 3433044225, "SQEX.Ebony.AIGraph.Data.PropertyData.PropertyVector", 128, 64, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("endPosition_", 1641034046, "SQEX.Ebony.AIGraph.Data.PropertyData.PropertyVector", 192, 64, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("endOffset_", 2614378470, "SQEX.Ebony.AIGraph.Data.PropertyData.PropertyVector", 256, 64, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("direction_", 4006647919, "SQEX.Ebony.AIGraph.Data.PropertyData.PropertyVector", 320, 64, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("distance_", 3236486151, "SQEX.Ebony.AIGraph.Data.PropertyData.PropertyFloat", 384, 32, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("sphereRadius_", 1763032771, "SQEX.Ebony.AIGraph.Data.PropertyData.PropertyFloat", 416, 32, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("hasHit_", 4113841649, "SQEX.Ebony.AIGraph.Data.PropertyData.PropertyBool", 448, 32, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("hitPosition_", 4108044862, "SQEX.Ebony.AIGraph.Data.PropertyData.PropertyVector", 480, 64, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("hitDistance_", 2905122538, "SQEX.Ebony.AIGraph.Data.PropertyData.PropertyFloat", 544, 32, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			fieldProperties.AddProperty(new Property("timeout_", 269585269, "SQEX.Ebony.AIGraph.Data.PropertyData.PropertyFloat", 576, 32, 1, Property.PrimitiveType.ClassField, 0, (char)0));
			
			
			return fieldProperties;
        }

		
        private static BaseObject Construct()
        {
            return new AILeafNodeSphereCast();
        }
		
    }
}