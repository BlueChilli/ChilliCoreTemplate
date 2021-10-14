using ChilliCoreTemplate.Models;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web
{
    public abstract class MvcActionContainer
    {
        string _area;
        protected MvcActionContainer(string area)
        {
            _area = area;
            this.Build();
        }

        protected void Build()
        {
            foreach (var field in this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = field.GetValue(this) as MvcActionDefinition;
                value?.Build(field, this._area);
            }
        }

        protected static IMvcActionDefinition MvcAction()
        {
            return new MvcActionDefinition();
        }
    }

    internal class MvcActionDefinition : IMvcActionDefinition
    {
        RouteValueDictionary _routeValues;

        public MvcActionDefinition()
        {
            _routeValues = new RouteValueDictionary();
        }

        internal void Build(FieldInfo field, string area)
        {
            var nameParts = field.Name.Split("_");
            var controller = nameParts.Length > 0 ? nameParts[0] : "";
            var action = nameParts.Length > 1 ? nameParts[1] : "";
            if (nameParts.Length > 2) action = String.Join("", nameParts, 1, nameParts.Length - 1);

            _routeValues["area"] = area ?? String.Empty;
            _routeValues["controller"] = controller ?? String.Empty;
            _routeValues["action"] = action ?? String.Empty;
        }

        public IReadOnlyDictionary<string, object> GetRouteValueDictionary()
        {
            return _routeValues;
        }

        public IMvcActionDefinition AddRouteValues(object values)
        {
            var clone = this.CloneInternal();
            if (values != null)
            {
                var parsed = new RouteValueDictionary(values);
                foreach (var kvp in parsed)
                {
                    if (kvp.Key.Equals("area", StringComparison.OrdinalIgnoreCase)
                        || kvp.Key.Equals("controller", StringComparison.OrdinalIgnoreCase)
                        || kvp.Key.Equals("action", StringComparison.OrdinalIgnoreCase))
                        continue;

                    clone._routeValues[kvp.Key] = kvp.Value;
                }
            }

            return clone;
        }

        public IMvcActionDefinition AddRouteId(int id)
        {
            return AddRouteValues(new { Id = id });
        }

        private MvcActionDefinition CloneInternal()
        {
            var newInstance = (MvcActionDefinition)this.MemberwiseClone();
            newInstance._routeValues = new RouteValueDictionary(_routeValues);

            return newInstance;
        }
    }
}