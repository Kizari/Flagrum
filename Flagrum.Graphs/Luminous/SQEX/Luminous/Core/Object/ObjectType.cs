using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SQEX.Luminous.Core.Object
{
    public class ObjectType
    {
        private string Name { get; }
        private uint ThisType { get; }
        private ObjectType BaseType { get; }
        public Func<BaseObject> ConstructFunction2 { get; }
        public PropertyContainer PropertyContainer { get; }
        private uint FunctionCount { get; }
        private int ObjectSize { get; }

        /// <summary>
        /// All registered object types.
        /// </summary>
        private static IDictionary<string, ObjectType> objectTypes = new Dictionary<string, ObjectType>();

        public ObjectType(string name, uint thisType, ObjectType baseType, Func<BaseObject> constructFunction2, PropertyContainer propertyContainer, uint functionCount, int objectSize)
        {
            this.Name = name;
            this.ThisType = thisType;
            this.BaseType = baseType;
            this.ConstructFunction2 = constructFunction2;
            this.PropertyContainer = propertyContainer;
            this.FunctionCount = functionCount;
            this.ObjectSize = objectSize;

            this.Register();
        }

        private void Register()
        {
            objectTypes.Add(this.Name, this);
        }

        /// <summary>
        /// Find the object type with the given full name.
        /// </summary>
        /// <param name="fullName">The full name of the object type.</param>
        /// <returns>The object type, or null if not found.</returns>
        public static ObjectType FindByFullName(string fullName)
        {
            if (objectTypes.Count == 0)
            {
                Black.Main.BlackMain.Initialize();
            }

            if (objectTypes.ContainsKey(fullName))
            {
                return objectTypes[fullName];
            }

            return null;
        }
    }
}
