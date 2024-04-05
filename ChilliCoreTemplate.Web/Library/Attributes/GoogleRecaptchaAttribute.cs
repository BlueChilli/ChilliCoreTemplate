using ChilliCoreTemplate.Service.Api.Google;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.Library
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class GoogleRecaptchaAttribute : ActionFilterAttribute
    {
        private string token;

        public double Score { get; set; }

        public GoogleRecaptchaAttribute()
        {
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;

            var headerExists = httpContext.Request.Headers.TryGetValue("x-grecaptcha", out StringValues requestToken);
            if (!headerExists && httpContext.Request.HasFormContentType)
            {
                httpContext.Request.Form.TryGetValue("g-recaptcha-response", out requestToken);
            }
            if (requestToken.Count > 0)
            {
                token = requestToken.First();
            }

            var service = httpContext.RequestServices.GetService<GoogleRecaptchaService>();
            var result = await service.Validate(token, Score);

            if (!result.Success)
            {
                var env = httpContext.RequestServices.GetService<IWebHostEnvironment>();
                context.Result = env.IsProduction() ? new StatusCodeResult(401) : new ObjectResult(result.Error) { StatusCode = 401 };
                return;
            }

            var resultContext = await next();
        }
    }
}
