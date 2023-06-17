using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Models
{
    public abstract class Patchable : IPatchable
    {
        protected readonly HashSet<string> _setProperties = new HashSet<string>();
        public HashSet<string> SetProperties()
        {
            return new HashSet<string>(_setProperties);
        }
    }

    public interface IPatchable
    {
        public HashSet<string> SetProperties();

        public void PatchTo(object record)
        {
            foreach (var propertyName in SetProperties())
            {
                var fromType = this.GetType().GetProperty(propertyName).PropertyType;

                var toProperty = record.GetType().GetProperty(propertyName);
                if (toProperty == null) continue;

                var toType = toProperty.PropertyType;
                var objectType = typeof(Object);

                var fromValue = this.GetType().GetProperty(propertyName).GetValue(this);
                if (fromType != toType && fromType.BaseType == objectType && toType.BaseType == objectType)
                {
                    var toValue = toProperty.GetValue(record);
                    if (toValue == null)
                    {
                        toValue = Activator.CreateInstance(toType);
                        toProperty.SetValue(record, toValue);
                    }
                    Copy(fromValue, toValue);
                }
                else
                {
                    toProperty.SetValue(record, fromValue);
                }
            }
        }

        private static void Copy(object from, object to)
        {
            var parentProperties = from.GetType().GetProperties();
            var childProperties = to.GetType().GetProperties();

            foreach (var parentProperty in parentProperties)
            {
                foreach (var childProperty in childProperties)
                {
                    if (parentProperty.Name == childProperty.Name && childProperty.CanWrite) //record.GetType().GetProperty(propertyName).SetValue
                    {
                        childProperty.SetValue(to, parentProperty.GetValue(from));
                        break;
                    }
                }
            }
        }
    }

    public static class IPatchableUtils
    {
        public static void PatchTo(this IPatchable instance, object record)
        {
            instance.PatchTo(record);
        }

        public static bool IsSetProperty(this IPatchable instance, string property)
        {
            return instance.SetProperties().Contains(property);
        }
    }

}
