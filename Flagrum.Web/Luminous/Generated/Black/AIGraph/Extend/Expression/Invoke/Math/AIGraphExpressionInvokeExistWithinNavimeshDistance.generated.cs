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

namespace Black.AIGraph.Extend.Expression.Invoke.Math
{
    [Serializable, CodeDom.Compiler.GeneratedCode("Luminaire", "0.1")]
    public partial class AIGraphExpressionInvokeExistWithinNavimeshDistance : Black.AIGraph.Extend.Expression.Invoke.Math.AIGraphExpressionInvokeTargetSlotArgsBase
    {
        new public static ObjectType ObjectType { get; private set; }
        private static PropertyContainer fieldProperties;
		
		
        
        new public static void SetupObjectType()
        {
            if (ObjectType != null)
            {
                return;
            }

            var dummy = new AIGraphExpressionInvokeExistWithinNavimeshDistance();
            var properties = dummy.GetFieldProperties();

            ObjectType = new ObjectType("Black.AIGraph.Extend.Expression.Invoke.Math.AIGraphExpressionInvokeExistWithinNavimeshDistance", 0, Black.AIGraph.Extend.Expression.Invoke.Math.AIGraphExpressionInvokeExistWithinNavimeshDistance.ObjectType, Construct, properties, 0, 8);
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

            fieldProperties = new PropertyContainer("Black.AIGraph.Extend.Expression.Invoke.Math.AIGraphExpressionInvokeExistWithinNavimeshDistance", base.GetFieldProperties(), -661314921, 1567361057);
            
			
			
			
			
			return fieldProperties;
        }

		
        private static BaseObject Construct()
        {
            return new AIGraphExpressionInvokeExistWithinNavimeshDistance();
        }
		
    }
}