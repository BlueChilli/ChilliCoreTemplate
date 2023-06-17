using ChilliCoreTemplate.Models.Api;
using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web
{
    public static class MvcInfrastructureHelper
    {
        public static readonly PathString ApiPrefix = new PathString("/api");

        public static bool IsApiRequest(this HttpRequest request)
        {
            return request.Path.StartsWithSegments(ApiPrefix);
        }

        internal static async Task ExceptionHandler(HttpContext context, bool showErrors)
        {
            var feature = context.Features.Get<IExceptionHandlerPathFeature>();
            if (feature == null)
                return;

            var request = context.Request;
            if (request.IsApiRequest())
            {
                ErrorResult obj = null;
                if (showErrors)
                {
                    obj = ErrorResult.Create(feature.Error.Message, feature.Error.StackTrace);
                }
                else
                {
                    obj = ErrorResult.Create("Internal Server Error");
                }

                var result = new ObjectResult(obj) { StatusCode = (int)HttpStatusCode.InternalServerError };

                await context.ExecuteResultAsync(result);
            }
            else if (!request.IsAjaxRequest())
            {
                var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
                context.Response.Headers["Location"] = $"{baseUrl}/Error/Index";
                context.Response.StatusCode = (int)HttpStatusCode.Found;
            }
        }

        internal static async Task OnRedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
        {
            if (context.HttpContext.Request.IsApiRequest())
            {
                var obj = ErrorResult.Create($"Access denied -> {context.Request.GetDisplayUrl()}");
                var result = new ObjectResult(obj) { StatusCode = (int)HttpStatusCode.Unauthorized };

                await context.HttpContext.ExecuteResultAsync(result);
            }
            else if (context.HttpContext.Request.IsAjaxRequest())
            {
                context.Response.Headers["X-Ajax-Redirect"] = context.RedirectUri;
                context.Response.StatusCode = (int)HttpStatusCode.OK;
            }
            else
            {
                context.Response.Headers["Location"] = context.RedirectUri;
                context.Response.StatusCode = (int)HttpStatusCode.Found;
            }
        }

        internal static async Task OnRedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
        {
            if (context.HttpContext.Request.IsApiRequest())
            {
                var obj = ErrorResult.Create($"Unauthorized request -> {context.Request.GetDisplayUrl()}");
                var result = new ObjectResult(obj) { StatusCode = (int)HttpStatusCode.Unauthorized };

                await context.HttpContext.ExecuteResultAsync(result);
            }
            else if (context.HttpContext.Request.IsAjaxRequest())
            {
                context.Response.Headers["X-Ajax-Redirect"] = context.RedirectUri;
                context.Response.StatusCode = (int)HttpStatusCode.OK;
            }
            else
            {
                context.Response.Headers["Location"] = context.RedirectUri;
                context.Response.StatusCode = (int)HttpStatusCode.Found;
            }
        }

        internal static async Task NotFoundApiHandler(HttpContext context)
        {
            var obj = ErrorResult.Create($"request not found -> {context.Request.GetDisplayUrl()}");
            var result = new ObjectResult(obj) { StatusCode = (int)HttpStatusCode.NotFound };

            await context.ExecuteResultAsync(result);
        }
    }
}
