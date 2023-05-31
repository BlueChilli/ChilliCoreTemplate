using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.Controllers
{
    public static class ControllerExtensions
    {        
        public static Task LoginWithPrincipalAsync(this ControllerBase controller, UserDataPrincipal principal)
        {
            return controller.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }

        public static void LoginWithPrincipal(this ControllerBase controller, UserDataPrincipal principal)
        {
            TaskHelper.WaitSafeSync(() => controller.LoginWithPrincipalAsync(principal));
        }

        public static async Task LoginWithPrincipalCookielessAsync(this ControllerBase controller, UserDataPrincipal principal)
        {
            var serviceProvider = controller.HttpContext.RequestServices;
            var optionsSnapshot = serviceProvider.GetRequiredService<IOptionsSnapshot<CookieAuthenticationOptions>>();
            var cookieOptions = optionsSnapshot.Get(CookieAuthenticationDefaults.AuthenticationScheme);
            var sessionSvc = serviceProvider.GetRequiredService<UserSessionService>();
            var settings = serviceProvider.GetRequiredService<ProjectSettings>();

            var expiry = principal.UserData.UserDeviceId != null ? TimeSpan.FromHours(settings.SessionLengthDevice) : cookieOptions.ExpireTimeSpan;
            principal.Id = await sessionSvc.CreateAsync(principal.UserData, DateTime.UtcNow.Add(expiry));
        }

        public static void LoginWithPrincipalCookieless(this ControllerBase controller, UserDataPrincipal principal)
        {
            TaskHelper.WaitSafeSync(() => controller.LoginWithPrincipalCookielessAsync(principal));
        }

        internal static async Task LogoutPrincipalAsync(this ControllerBase controller)
        {
            if (controller.User?.Identity?.IsAuthenticated == true)
            {
                var sessionSvc = controller.HttpContext.RequestServices.GetRequiredService<UserSessionService>();

                //calling delete manually because of cookieless sessions
                await sessionSvc.DeleteAsync(controller.User.Session()?.Id);
                await controller.HttpContext.SignOutAsync();
            }
        }

        public static ActionResult RedirectToRoot(this Controller c, ProjectSettings _settings, UserDataPrincipal ticket = null)
        {
            var user = ticket ?? c.User;
            if (user.IsAuthenticated())
            {
                if (user.UserData().IsInRole(Role.Administrator))
                    return Mvc.Admin.Default.Redirect(c);
                if (user.UserData().IsInRole(Role.CompanyAdmin))
                    return Mvc.Company.Default.Redirect(c);
                //else if (user.UserData().CurrentRoles.Any(r => RoleHelper.IsCompanyRole(r.Role)))
                //    return Mvc.Company.User_List.Redirect(c);
            }
            if (_settings.Hosting.UseIndexHtml)
                return c.Redirect(c.Url.Content("~/index.html"));
            else
                return Mvc.Root.EmailAccount_Login.Redirect(c);
        }

    }
}
