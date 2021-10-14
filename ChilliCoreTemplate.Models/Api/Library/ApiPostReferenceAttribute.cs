using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Humanizer;

namespace ChilliCoreTemplate.Models.Api
{
    public class ApiPostReferenceAttribute : Attribute
    {
        public ApiPostReferenceAttribute()
        {
        }

        public ApiPostReferenceAttribute(string postIdPropertyName)
        {
            this.PostIdPropertyName = postIdPropertyName;
        }

        string _PostIdPropertyName;
        public string PostIdPropertyName { get { return _PostIdPropertyName; } set { _PostIdPropertyName = value?.Trim(); } }

        public string GetPostIdPropertyNameOrDefault(PropertyInfo apiProperty)
        {
            if (!String.IsNullOrEmpty(PostIdPropertyName))
                return PostIdPropertyName;

            //list type
            if (apiProperty.PropertyType.IsArray || typeof(IEnumerable).IsAssignableFrom(apiProperty.PropertyType))
            {
                return $"{apiProperty.Name.Singularize()}Ids";
            }
            else
            {
                return $"{apiProperty.Name}Id";
            }
        }        
    }
}
