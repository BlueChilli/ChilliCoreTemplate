using ChilliCoreTemplate.Models.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.Api
{
    public class ApiKeyActionFilter : IAsyncActionFilter
    {
        private const string apiKeyHeaderKey = "ApiKey";        
        private string _apiKey;

        public ApiKeyActionFilter(string apiKey)
        {
            if (String.IsNullOrEmpty(apiKey))
            {
                throw new ApplicationException("ApiKey value is not set in ApiKeyActionFilter");
            }

            _apiKey = apiKey;
        }

        private bool ShouldCheckApiKey(HttpContext context)
        {
            if (!context.Request.IsApiRequest())
            {
                return false;
            }

            if (context.Items.ContainsKey(ApiKeyIgnoreAttribute.ShouldIgnoreApiKey))
            {
                if ((bool)context.Items[ApiKeyIgnoreAttribute.ShouldIgnoreApiKey])
                {
                    return false;
                }
            }

            return true;
        }

        public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (ShouldCheckApiKey(context.HttpContext))
            {
                var apiKey = context.HttpContext.Request.Headers[apiKeyHeaderKey].FirstOrDefault();
                apiKey = apiKey ?? context.HttpContext.Request.Headers[apiKeyHeaderKey.ToLower()].FirstOrDefault();
                apiKey = apiKey ?? context.HttpContext.Request.Query[apiKeyHeaderKey].FirstOrDefault();

                if (string.Compare(_apiKey, apiKey, StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    var error = ErrorResult.Create("Api key is invalid");
                    context.Result = new ObjectResult(error) { StatusCode = (int)HttpStatusCode.BadRequest };
                    return Task.CompletedTask;
                }
            }

            return next();
        }
    }
}
