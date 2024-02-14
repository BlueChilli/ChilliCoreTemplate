using ChilliCoreTemplate.Models.EmailAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.Api
{
    public class CompanyKeyMiddleware
    {
        private const string companyKeyHeaderKey = "CompanyKey";
        private readonly RequestDelegate _next;

        public CompanyKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var companyKey = httpContext.Request.Headers[companyKeyHeaderKey].FirstOrDefault();
            companyKey = companyKey ?? httpContext.Request.Headers[companyKeyHeaderKey.ToLower()].FirstOrDefault();
            companyKey = companyKey ?? httpContext.Request.Query[companyKeyHeaderKey].FirstOrDefault();

            if (Guid.TryParse(companyKey, out var guid))
            {
                var service = httpContext.RequestServices.GetRequiredService<Service.EmailAccount.UserSessionService>();
                var userData = await service.GetCompanySession(guid);

                if (userData != null)
                {
                    httpContext.User = new UserDataPrincipal(userData)
                    {
                        Id = guid.ToString()
                    };
                }
            }

            await _next.Invoke(httpContext);
        }
    }
}
