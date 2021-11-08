using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using ChilliCoreTemplate.Service.EmailAccount;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Service;
using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Authorization;
using ChilliSource.Cloud.Core;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Web;
using ChilliSource.Core.Extensions;
using System.Security.Principal;
using Microsoft.Extensions.DependencyInjection;

namespace ChilliCoreTemplate.Web.Controllers
{
    public class EmailAccountController : Controller
    {
        AccountService _accountService;
        ProjectSettings _settings;

        public UserData UserData
        {
            get
            {
                return User.UserData();
            }
        }

        public EmailAccountController(AccountService accountService, ProjectSettings settings)
        {
            _accountService = accountService;
            _settings = settings;
        }

        public virtual async Task<ActionResult> Login(string returnUrl = "", string email = "")
        {
            ModelState.Clear();

            if (User.Identity.IsAuthenticated)
            {
                await this.LogoutPrincipalAsync();
                //redirect required by ValidateAntiForgeryTokenAttribute
                return Mvc.Root.EmailAccount_Login.Redirect(this, routeValues: new { returnUrl, email });
            }

            var model = new SessionEditModel()
            {
                Email = email,
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [HttpPost, ActionName("Login")]
        [ValidateAntiForgeryToken]
        public virtual ActionResult LoginPost(SessionEditModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var loginResult = _accountService.Login(model, this.LoginWithPrincipal);

            if (!loginResult.Success)
            {
                if (loginResult.Error == "ResendRegistrationCompleteEmail")
                {
                    return Mvc.Root.EmailAccount_RegistrationActivationSent.Redirect(this, routeValues: new { email = model.Email });
                }
                else
                {
                    ModelState.AddModelError("Login failed", loginResult.Error);
                }

                return View(model);
            }

            return RedirectionAfterLogin(loginResult.Result, model.ReturnUrl);
        }

        private ActionResult RedirectionAfterLogin(UserDataPrincipal ticket = null, string returnUrl = null)
        {
            if (!String.IsNullOrEmpty(returnUrl))
            {
                if (returnUrl.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase) && !ticket.UserData.IsInRole(Role.Administrator)) return this.RedirectToRoot(_settings, ticket);
                if (returnUrl.StartsWith("/Company", StringComparison.OrdinalIgnoreCase) && ticket.UserData.IsInRole(Role.Administrator)) return this.RedirectToRoot(_settings, ticket);

                var url = $"{_settings.BaseUrl}{returnUrl}";
                url = String.Join('/', url.Split('/').Distinct());
                return this.Redirect(url);
            }

            return this.RedirectToRoot(_settings, ticket);
        }

        [HttpPost]
        public virtual ActionResult LoginWithToken([FromBody] EmailTokenModel model)
        {
            return this.ServiceCall(() => _accountService.LoginWithToken(model, this.LoginWithPrincipal))
                .OnSuccess(m =>
                {
                    return RedirectionAfterLogin(m);
                })
                .Call();
        }

        public virtual async Task<ActionResult> Logout()
        {
            await this.LogoutPrincipalAsync();
            return Mvc.Root.Public_Index.Redirect(this);
        }

        //[Authorize]
        //public virtual ActionResult ChooseRole()
        //{
        //    var accountId = UserData.UserId;
        //    var list = _accountService.GetListOfRoles(accountId);

        //    if (list.Count == 0)
        //        return this.Logout();

        //    if (list.Count == 1)
        //    {
        //        //var result = _accountService.SelectRole(accountId, list.First());
        //        //if (result.Success)
        //        //    return RedirectionAfterLogin();
        //    }
        //    else
        //        return View("ChooseRole", list);

        //    return this.Logout();
        //}

        //[Authorize]
        //[HttpPost]
        //public virtual ActionResult SelectRole(string jsonRole)
        //{
        //    var accountId = UserData.UserId;
        //    if (String.IsNullOrEmpty(jsonRole))
        //        return Mvc.Root.EmailAccount_ChooseRole.Redirect(this);

        //    var role = HttpUtility.HtmlDecode(jsonRole).FromJson<LoginRoleModel>();

        //    var response = _accountService.SelectRole(accountId, role);
        //    if (!response.Success)
        //        return Mvc.Root.EmailAccount_ChooseRole.Redirect(this);

        //    return RedirectionAfterLogin();
        //}

        private const Role _registrationRole = Role.CompanyAdmin;
        public virtual ActionResult Registration(RegistrationViewModel model)
        {
            ModelState.Clear();
            model.MixpanelTempId = Guid.NewGuid();
            model.Roles = _registrationRole;
            Mixpanel.SendEventToMixpanel(model.MixpanelTempId.ToString(), "Signup form");
            return View(model);
        }

        [HttpPost, ActionName("Registration")]
        [ValidateAntiForgeryToken]
        public virtual ActionResult RegistrationPost(RegistrationViewModel model)
        {
            if (model != null) model.Roles = _registrationRole;
            return this.ServiceCall(() => _accountService.Create(model))
                .OnSuccess(m =>
                {
                    //Choice activate now, or log user on - some functions may need to be blocked when not activated
                    return Mvc.Root.EmailAccount_RegistrationActivationSent.Redirect(this, routeValues: new { email = model.Email });

                    //_accountService.Session_Create(m.UserId).SetLoginCookie();
                    //return Menu.Company.Redirect();
                })
                .OnFailure(() =>
                {
                    return View(model);
                })
                .Call();
        }

        public virtual ActionResult RegistrationActivationSent(string email)
        {
            return View("RegistrationActivationSent", email);
        }

        [CustomAuthorize]
        [HttpPost]
        public virtual ActionResult ResendActivationEmail()
        {
            this._accountService.SendRegistrationCompleteEmail(UserData.UserId);
            return Json(new { result = "ok" });
        }

        public virtual ActionResult RegistrationComplete(EmailTokenModel model)
        {
            if (ModelState.IsValid)
            {
                var result = _accountService.Activate(model);
                if (result.Success)
                {
                    TempData[PageMessage.Key()] = PageMessage.Success("Thanks! Your account has been successfully activated.");
                    if (User.IsAuthenticated())
                    {
                        return Mvc.Root.Entry_Index.Redirect(this);
                    }
                    else
                    {
                        return LoginWithToken(model);
                    }
                }
                else
                {
                    ModelState.AddResult(result);
                }
            }
            return View(model);
        }

        public virtual ActionResult ConfirmInvite(EmailTokenModel model)
        {
            var viewModel = new ResetPasswordViewModel { Token = model.Token, Email = model.Email };
            return View(viewModel);
        }

        [HttpPost, ActionName("ConfirmInvite")]
        public virtual ActionResult ConfirmInvitePost(ResetPasswordViewModel model)
        {
            return this.ServiceCall(() => _accountService.ConfirmInvite(model))
                .OnSuccess(m =>
                {
                    return Mvc.Root.EmailAccount_ConfirmInviteSuccess.Redirect(this, routeValues: new { Email = model.Email });

                })
                .OnFailure(() =>
                {
                    return View(model);
                })
                .Call();
        }

        public virtual ActionResult ConfirmInviteSuccess(string email)
        {
            return View("ConfirmInviteSuccess", model: email);
        }

        public virtual ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult ForgotPassword(ResetPasswordRequestModel model)
        {
            return this.ServiceCall(() => _accountService.Password_ResetRequest(model))
                .OnSuccess(m =>
                {
                    return Mvc.Root.EmailAccount_ForgotPasswordSent.Redirect(this, routeValues: new { Email = model.Email });
                })
                .OnFailure(() =>
                {
                    return View(model);
                })
                .Call();
        }

        public virtual ActionResult ForgotPasswordSent(ResetPasswordRequestModel model)
        {
            return View(model);
        }

        public virtual ActionResult ResetPassword(string token, string email)
        {
            ResetPasswordViewModel viewModel = new ResetPasswordViewModel { Token = token, Email = email };

            return View(viewModel);
        }

        [HttpPost]
        public virtual ActionResult ResetPassword(ResetPasswordViewModel viewModel)
        {
            viewModel.Success = false;

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var result = _accountService.Password_Reset(viewModel);

            if (!result.Success)
            {
                ModelState.AddModelError("Update failed", result.Error);

                return View(viewModel);
            }

            return Mvc.Root.EmailAccount_ResetPasswordSuccess.Redirect(this, routeValues: new { Email = viewModel.Email });
        }

        public virtual ActionResult ResetPasswordSuccess(string email)
        {
            return View("ResetPasswordSuccess", model: email);
        }

        [CustomAuthorize]
        public virtual ActionResult ChangeDetails()
        {
            return this.ServiceCall(() => _accountService.GetForEdit(UserData.UserId))
                 .Always(m =>
                 {
                     return View(m);
                 })
                 .Call();
        }

        [CustomAuthorize, HttpPost, ActionName("ChangeDetails")]
        [ValidateAntiForgeryToken]
        public virtual ActionResult ChangeDetailsPosted(AccountDetailsEditModel model)
        {
            return this.ServiceCall(() => _accountService.Update(model, UserData.UserId))
                .OnSuccess(m =>
                {
                    TempData[PageMessage.Key()] = PageMessage.Success("Your account details have been successfully updated");

                    return Mvc.Root.EmailAccount_ChangeDetails.Redirect(this);
                })
                .OnFailure(() =>
                {
                    return View(model);
                })
                .Call();
        }

        [CustomAuthorize]
        public virtual ActionResult ChangePassword()
        {
            ChangePasswordViewModel viewModel = new ChangePasswordViewModel();

            return View(viewModel);
        }

        [CustomAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            model.UserId = UserData.UserId;
            return this.ServiceCall(() => _accountService.Password_Change(model))
                .OnSuccess(m =>
                {
                    TempData[PageMessage.Key()] = PageMessage.Success("Your password has been successfully updated");
                    return Mvc.Root.EmailAccount_ChangePassword.Redirect(this);
                })
                .OnFailure(() =>
                {
                    return View(model);
                })
                .Call();
        }

        [CustomAuthorize]
        public virtual async Task<ActionResult> Cancel()
        {
            int accountId = UserData.UserId;
            await _accountService.SoftDeleteAsync(accountId);

            return await Logout();
        }

        const string clearGif1X1 = "R0lGODlhAQABAIAAAP///wAAACH5BAEAAAAALAAAAAABAAEAAAICRAEAOw==";

        [ResponseCache(NoStore = true, Duration = 0)]
        public FileContentResult EmailOpen(ShortGuid? emailId = null)
        {
            if (emailId != null && emailId.Value.Guid != Guid.Empty)
                _accountService.Email_Open(emailId.Value);

            return new FileContentResult(Convert.FromBase64String(clearGif1X1), "image/gif");
        }

        public virtual ActionResult EmailDummyOpen(ShortGuid emailId)
        {
            return new FileContentResult(Convert.FromBase64String(clearGif1X1), "image/gif");
        }

        public virtual ActionResult EmailRedirect(ShortGuid? emailId, string url = null)
        {
            if (emailId == null || url == null)
            {
                return new RedirectResult(_settings.PublicUrl);
            }

            if (_accountService.Email_Clicked(emailId.Value).Success)
            {
                return new RedirectResult(url);
            }

            return new RedirectResult(_settings.PublicUrl);
        }

        //public ActionResult EmailUnsubscribe(ShortGuid id)
        //{
        //    return this.ServiceCall(() => _accountService.Email_GetForUnsubscribe(id)).Call();
        //}

        //[HttpPost]
        //public ActionResult EmailUnsubscribe(EmailUnsubscribeModel email)
        //{
        //    return this.ServiceCall(() => _accountService.Email_Unsubscribe(email))
        //        .OnSuccess(m =>
        //        {
        //            TempData[PageMessage.Key()] = PageMessage.Success("You have been successfully unsubscribed from this email.";
        //            return View(email);
        //        })
        //        .OnFailure(() => { return View(email); })
        //        .Call();
        //}

    }
}
