using System.Diagnostics;
using System.Reflection;

namespace SQEX.Luminous.Core.Object
{
    public class BaseObject : Ebony.Base.Allocatable
    {
        private static ObjectType objectType;

        public static ObjectType GetObjectTypeStatic()
        {
            if (objectType != null)
            {
                return objectType;
            }

            objectType = new ObjectType("BaseObject", 111, null, Construct, null, 0, -1);
            return objectType;
        }

        public void SetPropertyValue(Property property, object val)
        {
            var name = property.Name;
            var prop = this.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            if (prop?.CanWrite == true)
            {
                prop.SetValue(this, val, null);
            }

            // TODO HACK since Unity hates properties
            if (prop == null)
            {
                var field = this.GetType().GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                if (field == null)
                {
                    return;
                }

                field.SetValue(this, val);
            }
        }

        public T GetPropertyValue<T>(Property property)
        {
            var name = property.Name;
            var prop = this.GetType().GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            return (T)prop.GetValue(this);
        }

        public virtual ObjectType GetObjectType()
        {
            return objectType;
        }

        protected virtual PropertyContainer GetFieldProperties()
        {
            return null;
        }

        private static BaseObject Construct()
        {
            return new BaseObject();
        }
    }
}