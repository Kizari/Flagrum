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

namespace SQEX.Luminous.Core.Object
{
    [Serializable, CodeDom.Compiler.GeneratedCode("Luminaire", "0.1")]
    public partial class Object : BaseObject
    {
        new public static ObjectType ObjectType { get; private set; }
        private static PropertyContainer fieldProperties;
		
		
        
        new public static void SetupObjectType()
        {
            if (ObjectType != null)
            {
                return;
            }

            var dummy = new Object();
            var properties = dummy.GetFieldProperties();

            ObjectType = new ObjectType("SQEX.Luminous.Core.Object.Object", 0, SQEX.Luminous.Core.Object.Object.ObjectType, Construct, properties, 1, 16);
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

            fieldProperties = new PropertyContainer("SQEX.Luminous.Core.Object.Object", base.GetFieldProperties(), 254102316, 1792615884);
            
			
			
			
			
			return fieldProperties;
        }

		
        private static BaseObject Construct()
        {
            return new Object();
        }
		
    }
}