using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.Api
{
    public class ApiKeyIgnoreAttribute : Attribute, IAuthorizationFilter
    {
        public const string ShouldIgnoreApiKey = "_CC7F91129100_ShouldIgnoreApiKey";

        public ApiKeyIgnoreAttribute(bool value = true)
        {
            this.Value = value;
        }

        public bool Value { get; }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            context.HttpContext.Items[ShouldIgnoreApiKey] = Value;
        }
    }
}
