using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Web.Controllers;

namespace ChilliCoreTemplate.Web
{
    public class MfaAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var config = context.HttpContext.RequestServices.GetService<ProjectSettings>();
            if (!config.MfaSettings.Enabled) return;

            var userData = context.HttpContext.User?.UserData();
            if (userData == null) return;

            if (userData.IsMfaVerified || (userData.IsImpersonated() && userData.Impersonator.IsMfaVerified))
                return;

            context.Result = new RedirectToActionResult(nameof(MfaController.Entry), "Mfa", new { area = "" });
        }
    }
}
