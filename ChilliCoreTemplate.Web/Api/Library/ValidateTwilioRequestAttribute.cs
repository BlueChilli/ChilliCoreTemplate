using ChilliCoreTemplate.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Twilio.Security;

namespace ChilliCoreTemplate.Web.Api.Library
{
    //https://github.com/TwilioDevEd/csharp-web-api-validation/blob/master/ValidateRequestExample/Filters/ValidateTwilioRequestImprovedFilter.cs

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ValidateTwilioRequestAttribute : ActionFilterAttribute
    {
        private ProjectSettings _config { get; set; }
        private IWebHostEnvironment _env { get; set; }
        private string _authToken => _config.SmsSettings.Password;
        private string _urlSchemeAndDomain => _config.BaseUrl;

        public ValidateTwilioRequestAttribute()
        {
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            _env = context.HttpContext.RequestServices.GetService<IWebHostEnvironment>();
            _config = context.HttpContext.RequestServices.GetService<ProjectSettings>();

            if (!await IsValidRequestAsync(context.HttpContext.Request) && !_env.IsDevelopment())
            {
                context.Result = new StatusCodeResult(403);
            }

            var resultContext = await next();
        }

        private async Task<bool> IsValidRequestAsync(HttpRequest request)
        {
            var headerExists = request.Headers.TryGetValue(
                "X-Twilio-Signature", out StringValues signature);
            if (!headerExists) return false;

            var requestUrl = _urlSchemeAndDomain + request.GetEncodedPathAndQuery();
            var formData = (await request.ReadFormAsync()).ToDictionary(k => k.Key, v => v.Value.FirstOrDefault());
            return new RequestValidator(_authToken).Validate(requestUrl, formData, signature.First());
        }

        private async Task<IDictionary<string, string>> GetFormDataAsync(HttpContent content)
        {
            string postData;
            using (var stream = new StreamReader(await content.ReadAsStreamAsync()))
            {
                stream.BaseStream.Position = 0;
                postData = await stream.ReadToEndAsync();
            }

            if (!String.IsNullOrEmpty(postData) && postData.Contains("="))
            {
                return postData.Split('&')
                    .Select(x => x.Split('='))
                    .ToDictionary(
                        x => Uri.UnescapeDataString(x[0]),
                        x => Uri.UnescapeDataString(x[1].Replace("+", "%20"))
                    );
            }

            return new Dictionary<string, string>();
        }

    }
}
