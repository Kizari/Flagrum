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

namespace SQEX.Ebony.AIGraph.Core
{
    [Serializable, CodeDom.Compiler.GeneratedCode("Luminaire", "0.1")]
    public partial class AIGraphCompiledExpression : SQEX.Luminous.Core.Object.Object
    {
        new public static ObjectType ObjectType { get; private set; }
        private static PropertyContainer fieldProperties;
		
		public string compiledExpression_= string.Empty;
		public string debugRawExpression_= string.Empty;
		
        
        new public static void SetupObjectType()
        {
            if (ObjectType != null)
            {
                return;
            }

            var dummy = new AIGraphCompiledExpression();
            var properties = dummy.GetFieldProperties();

            ObjectType = new ObjectType("SQEX.Ebony.AIGraph.Core.AIGraphCompiledExpression", 0, SQEX.Ebony.AIGraph.Core.AIGraphCompiledExpression.ObjectType, Construct, properties, 0, 56);
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

            fieldProperties = new PropertyContainer("SQEX.Ebony.AIGraph.Core.AIGraphCompiledExpression", base.GetFieldProperties(), -357804267, -855782566);
            
			
			
			fieldProperties.AddProperty(new Property("compiledExpression_", 1854894313, "Ebony.Base.String", 16, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			fieldProperties.AddProperty(new Property("debugRawExpression_", 3705700521, "Ebony.Base.String", 32, 16, 1, Property.PrimitiveType.String, 0, (char)0));
			
			
			return fieldProperties;
        }

		
        private static BaseObject Construct()
        {
            return new AIGraphCompiledExpression();
        }
		
    }
}