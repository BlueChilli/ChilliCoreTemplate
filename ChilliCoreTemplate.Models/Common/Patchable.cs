using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            PatchTo(this, record, SetProperties());
        }

        private static void PatchTo(object from, object to, HashSet<string> setProperties, bool isChild = false)
        {
            foreach (var propertyName in setProperties)
            {
                var fromProperty = from.GetType().GetProperty(propertyName);
                var fromType = fromProperty.PropertyType;

                var toPropertyName = propertyName;

                var command = fromProperty.GetCustomAttribute<PatchAttribute>();
                if (command != null && !isChild)
                {
                    switch (command.Target)
                    {
                        case PatchTarget.Root:
                            Copy(fromProperty.GetValue(from), to);
                            continue;
                        case PatchTarget.Current:
                            toPropertyName = command.Value;
                            break;
                        case PatchTarget.Child:
                            var targetProperty = to.GetType().GetProperty(command.Value);
                            if (targetProperty != null)
                            {
                                var target = GetOrCreateValue(targetProperty, to);
                                PatchTo(from, target, new HashSet<string> { propertyName }, isChild: true);
                            }
                            continue;
                    }
                }

                var toProperty = to.GetType().GetProperty(toPropertyName);
                if (toProperty == null) continue;

                var toType = toProperty.PropertyType;
                var objectType = typeof(Object);

                var fromValue = fromProperty.GetValue(from);
                if (fromValue != null && fromType != toType && fromType.BaseType == objectType && toType.BaseType == objectType)
                {
                    var toValue = GetOrCreateValue(toProperty, to);
                    Copy(fromValue, toValue);
                }
                else
                {
                    toProperty.SetValue(to, fromValue);
                }
            }
        }

        private static object GetOrCreateValue(PropertyInfo property, object model)
        {
            var result = property.GetValue(model);
            if (result == null)
            {
                result = Activator.CreateInstance(property.PropertyType);
                property.SetValue(model, result);
            }
            return result;
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

    [AttributeUsage(validOn: AttributeTargets.Property)]
    public class PatchAttribute : Attribute
    {
        private PatchTarget _target;
        public PatchTarget Target => _target;

        public string Value { get; set; }

        public PatchAttribute(PatchTarget target)
        {
            _target = target;
        }
    }

    public enum PatchTarget
    {
        Root,
        Current,
        Child
    }

}
