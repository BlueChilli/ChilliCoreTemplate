using ChilliCoreTemplate.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.Api
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class PrivateApiKeyAttribute : ActionFilterAttribute
    {
        public PrivateApiKeyType Type { get; }

        public PrivateApiKeyAttribute(PrivateApiKeyType type)
        {
            this.Type = type;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var config = context.HttpContext.RequestServices.GetService<ProjectSettings>();

            var headerExists = context.HttpContext.Request.Headers.TryGetValue("X-Private-Key", out StringValues privateApiKey);
            if (headerExists)
            {
                var key = privateApiKey.First();
                if (!String.IsNullOrEmpty(key))
                {
                    if (config.ApiSettings.PrivateApiKeys[Type] == key)
                    {
                        await next();
                        return;
                    }
                }
            }

            context.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
        }
    }
}
