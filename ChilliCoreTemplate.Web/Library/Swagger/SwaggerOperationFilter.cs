using ChilliCoreTemplate.Web.Api;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ChilliCoreTemplate.Web
{
    public class SwaggerOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;
            var apiParameters = apiDescription.ParameterDescriptions.Where(p => p.ParameterDescriptor != null)
                                    .ToLookup(p => p.ParameterDescriptor.Name);

            //TODO => https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1296
            //{Produces("application/json", "application/xml", Type = typeof(FooBar))}
            //[Consumes("application/json")]
            //operation.Consumes = new List<string>() { "application/json", "multipart/form-data" };

            var apiKeyIgnore = GetControllerAndActionAttributes<ApiKeyIgnoreAttribute>(apiDescription).LastOrDefault();
            if (apiKeyIgnore == null || apiKeyIgnore?.Value == false)
            {
                operation.Security ??= new List<OpenApiSecurityRequirement>();

                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "apiKeyHeader" }
                        },
                        new string[] { }
                    }
                });
            }

            if (GetControllerAndActionAttributes<CustomAuthorizeAttribute>(apiDescription).Any())
            {
                operation.Security ??= new List<OpenApiSecurityRequirement>();

                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "userKeyHeader" }
                        },
                        new string[] { }
                    }
                });
            }

        }

        private IEnumerable<T> GetControllerAndActionAttributes<T>(ApiDescription apiDescription) where T : Attribute
        {
            MethodInfo methodInfo;
            if (!apiDescription.TryGetMethodInfo(out methodInfo))
            {
                return Enumerable.Empty<T>();
            }

            return methodInfo.DeclaringType.GetCustomAttributes<T>()
                    .Concat(methodInfo.GetCustomAttributes<T>());
        }
    }
}
