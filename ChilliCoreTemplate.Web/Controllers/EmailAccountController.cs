using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Api.OAuth;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public virtual async Task<ActionResult> Login(string returnUrl = "", string email = "", string error = "")
        {
            ModelState.Clear();

            if (!String.IsNullOrEmpty(error)) ModelState.AddModelError("Login failed", error);

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
            model.OAuthUrls = OAuthUrls();

            return View(model);
        }

        private Dictionary<OAuthProvider, string> OAuthUrls()
        {
            var urls = new Dictionary<OAuthProvider, string>();

            urls.Add
            (
                OAuthProvider.Google,
                _accountService.OAuth_Url(new OAuthUrlApiModel
                {
                    Provider = OAuthProvider.Google,
                    RedirectUrl = Mvc.Root.EmailAccount_LoginOAuth.Url(this)
                }, OAuthMode.Login).Result
            );

            urls.Add
            (
                OAuthProvider.Apple,
                _accountService.OAuth_Url(new OAuthUrlApiModel
                {
                    Provider = OAuthProvider.Apple,
                    RedirectUrl = Mvc.Root.EmailAccount_LoginOAuth.Url(this)
                }, OAuthMode.Login).Result
            );

            return urls;
        }

        [ValidateAntiForgeryToken]
        [HttpPost, ActionName("Login")]
        public virtual ActionResult LoginPost(SessionEditModel model)
        {
            if (!ModelState.IsValid)
            {
                model.OAuthUrls = OAuthUrls();
                return View(model);
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
                model.OAuthUrls = OAuthUrls();
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

        public virtual ActionResult LoginOAuth(string email, string token, string error, string errorDescription)
        {
            if (!String.IsNullOrEmpty(error) || !String.IsNullOrEmpty(errorDescription))
            {
                ModelState.AddModelError("OAuth", errorDescription ?? error);
                return View("Login", new SessionEditModel { OAuthUrls = OAuthUrls() });
            }
            return LoginWithToken(new UserTokenModel { Email = email, Token = token });
        }

        private ActionResult LoginWithToken(UserTokenModel model)
        {
            return this.ServiceCall(() => _accountService.LoginWithToken(model, this.LoginWithPrincipal))
                .OnSuccess(m =>
                {
                    return RedirectionAfterLogin(m);
                })
                .OnFailure(m => Mvc.Root.EmailAccount_Login.Redirect(this, new { error = ModelState.Errors().First().Value }))
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
            model.OAuthUrls = OAuthUrls();
            Mixpanel.SendEventToMixpanel(model.MixpanelTempId.ToString(), "Signup form");
            return View(model);
        }

        [HttpPost, ActionName("Registration")]
        [ValidateAntiForgeryToken]
        public virtual ActionResult RegistrationPost([FromForm] RegistrationViewModel model)
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

        [HttpPost, ActionName("RegistrationOAuth")]
        public virtual ActionResult RegistrationOAuthPost(RegistrationOAuthModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Registration", new RegistrationViewModel { Roles = _registrationRole });
            }
            var url = _accountService.OAuth_Url(new OAuthUrlApiModel
            {
                Provider = model.Provider.Value,
                RedirectUrl = Mvc.Root.EmailAccount_RegistrationOAuth.Url(this)
            },
                OAuthMode.Register,
                $"{model.CompanyName}:{_registrationRole}"
            ).Result;
            return new RedirectResult(url);
        }

        public virtual ActionResult RegistrationOAuth(string email, string token, string error, string errorDescription)
        {
            if (!String.IsNullOrEmpty(error) || !String.IsNullOrEmpty(errorDescription))
            {
                ModelState.AddModelError("OAuth", errorDescription ?? error);
                return View("Registration", new RegistrationViewModel { Roles = _registrationRole });
            }
            return LoginWithToken(new UserTokenModel { Email = email, Token = token });
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

        public virtual ActionResult RegistrationComplete(UserTokenModel model)
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

        public virtual ActionResult ConfirmInvite(UserTokenModel model)
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

        public virtual ActionResult ForgotPassword(string email)
        {
            return View(new ResetPasswordRequestModel { Email = email });
        }

        [HttpPost, ActionName("ForgotPassword")]
        [ValidateAntiForgeryToken]
        public virtual ActionResult ForgotPassword(ResetPasswordRequestModel model)
        {
            return this.ServiceCall(() => _accountService.Password_ResetRequest(model.Email))
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

        public virtual ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            ModelState.Clear();
            return View(model);
        }

        [HttpPost, ActionName("ResetPassword")]
        public virtual ActionResult ResetPasswordPost(ResetPasswordViewModel model)
        {
            model.Success = false;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = _accountService.Password_Reset(model);

            if (!result.Success)
            {
                ModelState.AddModelError("Update failed", result.Error);

                return View(model);
            }

            return Mvc.Root.EmailAccount_ResetPasswordSuccess.Redirect(this, routeValues: new { Email = model.Email, IsApi = model.IsApi });
        }

        public virtual ActionResult ResetPasswordSuccess(ResetPasswordViewModel model)
        {
            return View("ResetPasswordSuccess", model);
        }

        [CustomAuthorize]
        public virtual ActionResult ChangeDetails()
        {
            return this.ServiceCall(() => _accountService.GetForEdit(UserData.UserId))
                 .Always(m =>
                 {
                     return PartialView(m);
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
                    return PartialView(model);
                })
                .OnFailure(() =>
                {
                    return PartialView(model);
                })
                .Call();
        }

        [CustomAuthorize]
        public virtual ActionResult ChangePassword()
        {
            ChangePasswordViewModel viewModel = new ChangePasswordViewModel();

            return PartialView(viewModel);
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
                    return PartialView(model);
                })
                .OnFailure(() =>
                {
                    return PartialView(model);
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

        public virtual ActionResult EmailRedirectDummy(ShortGuid? emailId, string url = null)
        {
            if (emailId == null || url == null)
            {
                return new RedirectResult(_settings.PublicUrl);
            }
            return new RedirectResult(url);
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
