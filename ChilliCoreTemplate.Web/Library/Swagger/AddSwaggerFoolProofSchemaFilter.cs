using FoolProof.Core;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ChilliCoreTemplate.Web.Library.Swagger
{
    public class AddSwaggerFoolProofSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            //TODO make into a description if needed
            //if (context.ParameterInfo != null)
            //{
            //    var descriptionAttributes = context.ParameterInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            //    if (descriptionAttributes.Length > 0)
            //    {
            //        var descriptionAttribute = (DescriptionAttribute)descriptionAttributes[0];
            //        schema.Description = descriptionAttribute.Description;
            //    }
            //}

            //if (context.MemberInfo != null)
            //{
            //    var descriptionAttributes = context.MemberInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            //    if (descriptionAttributes.Length > 0)
            //    {
            //        var descriptionAttribute = (DescriptionAttribute)descriptionAttributes[0];
            //        schema.Description = descriptionAttribute.Description;
            //    }
            //}

            //if (context.Type != null)
            //{
            //    var descriptionAttributes = context.Type.GetCustomAttributes(typeof(DescriptionAttribute), false);

            //    if (descriptionAttributes.Length > 0)
            //    {
            //        var descriptionAttribute = (DescriptionAttribute)descriptionAttributes[0];
            //        schema.Description = descriptionAttribute.Description;
            //    }

            //}

            PropertyInfo[] properties = context.Type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                DescribeContingentValidationAttribute<RequiredIfEmptyAttribute>("empty", property, schema);
                DescribeContingentValidationAttribute<RequiredIfNotEmptyAttribute>("not empty", property, schema);
            }

        }

        private void DescribeContingentValidationAttribute<T>(string condition, PropertyInfo property, OpenApiSchema schema) where T : ContingentValidationAttribute
        {
            var attribute = property.GetCustomAttribute<T>();

            if (attribute == null) return;

            var dependantProperty = attribute.DependentProperty;

            if (attribute != null)
            {
                var propertyNameInCamelCasing = char.ToLowerInvariant(property.Name[0]) + property.Name.Substring(1);

                if (schema.Required == null)
                {
                    schema.Required = new HashSet<string>()
                        {
                            propertyNameInCamelCasing
                        };
                }
                else
                {
                    schema.Required.Add(propertyNameInCamelCasing);
                }

                var schemaProperty = schema.Properties.Where(x => x.Key == propertyNameInCamelCasing).FirstOrDefault();
                schemaProperty.Value.Description = $"Required if {dependantProperty} is {condition}";
            }
        }
    }
}
