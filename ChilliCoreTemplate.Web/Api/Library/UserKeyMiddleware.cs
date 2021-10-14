using ChilliCoreTemplate.Models.EmailAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.Api
{
    public class UserKeyMiddleware
    {
        private const string userKeyHeaderKey = "UserKey";
        private readonly RequestDelegate _next;

        public UserKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var userKey = httpContext.Request.Headers[userKeyHeaderKey].FirstOrDefault();
            userKey = userKey ?? httpContext.Request.Headers[userKeyHeaderKey.ToLower()].FirstOrDefault();
            userKey = userKey ?? httpContext.Request.Query[userKeyHeaderKey].FirstOrDefault();

            var userKeyHelper = httpContext.RequestServices.GetRequiredService<UserKeyHelper>();
            var sessionId = userKeyHelper.UnprotectGuid(userKey);
            if (sessionId != null)
            {
                var service = httpContext.RequestServices.GetRequiredService<Service.EmailAccount.UserSessionService>();
                var session = await service.GetAsync(sessionId.ToString(), httpContext.RequestAborted);

                if (session != null)
                {
                    httpContext.User = new UserDataPrincipal(session.UserData)
                    {
                        Id = session.Id
                    };
                }                
            }

            await _next.Invoke(httpContext);
        }
    }
}
