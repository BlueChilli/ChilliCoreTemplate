using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service.EmailAccount;
using System;
using ChilliSource.Core.Extensions;
using ChilliSource.Cloud.Core;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace ChilliCoreTemplate.Web.Controllers
{
    [CustomAuthorize]
    public class MfaController : Controller
    {
        private MfaService _service;
        private AccountService _accountService;
        private ProjectSettings _config;

        public MfaController(MfaService service, AccountService accountService, ProjectSettings config)
        {
            _service = service;
            _accountService = accountService;
            _config = config;
        }

        public ActionResult Index()
        {
            return Mvc.Root.Mfa_Entry.Redirect(this);
        }

        [HttpGet]
        public ActionResult Entry()
        {
            if (IsMfaVerified()) return Mvc.Root.Entry_Index.Redirect(this);

            if (_service.IsEnabled()) return Mvc.Root.Mfa_Confirm.Redirect(this);

            return Mvc.Root.Mfa_Enable.Redirect(this);
        }

        [HttpGet]
        public ActionResult Enable()
        {
            if (IsMfaVerified()) return Mvc.Root.Entry_Index.Redirect(this);

            return this.ServiceCall(_service.Setup)
                .OnFailure(Entry)
                .Call();
        }

        [HttpPost]
        public async Task<ActionResult> Enable(MfaSetupModel model)
        {
            return await this.ServiceCall(() => _service.Setup(model))
                .OnSuccess(() =>
                {
                    return Mvc.Root.Entry_Index.Redirect(this);
                })
                .OnFailure(Enable)
                .Call();
        }

        [HttpGet]
        public async Task<ActionResult> Confirm(string returnUrl = null)
        {
            if (IsMfaVerified() || await _service.ConfirmSkipCode(Request.Cookies[MfaConfirmModel.SkipCodeKey]))
            {
                if (!String.IsNullOrEmpty(returnUrl))
                {
                    var url = $"{_config.BaseUrl}{returnUrl}";
                    url = String.Join('/', url.Split('/').Distinct());
                    return this.Redirect(url);
                }
                return Mvc.Root.Entry_Index.Redirect(this);
            }

            if (!_service.IsEnabled()) return Mvc.Root.Mfa_Enable.Redirect(this);

            var model = new MfaConfirmModel();
            return View("MfaConfirm", model);
        }

        [HttpPost]
        public async Task<ActionResult> Confirm(MfaSetupModel model)
        {
            return await this.ServiceCall(() => _service.Confirm(model))
                .OnSuccess(() =>
                {
                    if (model.TrustDevice && _config.MfaSettings.TrustDeviceInDays.HasValue)
                    {
                        Response.Cookies.Append(MfaConfirmModel.SkipCodeKey, MfaConfirmModel.GetSkipCode(User.UserData(), _config), new CookieOptions
                        {
                            SameSite = SameSiteMode.Strict,
                            Secure = true,
                            Expires = DateTimeOffset.UtcNow.AddDays(_config.MfaSettings.TrustDeviceInDays.Value)
                        });
                    }
                    return Mvc.Root.Entry_Index.Redirect(this);
                })
                .OnFailure(() => Confirm())
                .Call();
        }

        [CustomAuthorize(Roles = AccountCommon.Administrator)]
        public ActionResult Remove(int id)
        {
            var user = _accountService.Get<AccountViewModel>(id, visibleOnly: true);
            return PartialView("MfaRemove", user);
        }

        [CustomAuthorize(Roles = AccountCommon.Administrator)]
        [HttpPost, ActionName("Remove")]
        public ActionResult RemovePost(int id)
        {
            return this.ServiceCall(() => _service.Remove(id))
                .OnSuccess(() =>
                {
                    return Mvc.Admin.User_Users_Details.Redirect(this, id);
                })
                .OnFailure(() => Remove(id))
                .Call();
        }

        private bool IsMfaVerified() => User.UserData().IsMfaVerified || (User.UserData().Impersonator?.IsMfaVerified ?? false);
    }
}