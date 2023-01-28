using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using ChilliSource.Cloud.Web.MVC;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Service.Admin;
using ChilliCoreTemplate.Service;
using ChilliCoreTemplate.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using DataTables.AspNet.Core;
using ChilliCoreTemplate.Models.EmailAccount;
using DataTables.AspNet.AspNetCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using ChilliSource.Core.Extensions;
using Newtonsoft.Json;
using ChilliCoreTemplate.Web.Controllers;
using ChilliSource.Cloud.Core;
using ChilliCoreTemplate.Models.Api;

namespace ChilliCoreTemplate.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [CustomAuthorize(Roles = AccountCommon.Administrator)]
    public class UserController : Controller
    {
        private AdminService _service;
        private CompanyService _companyService;
        private AccountService _accountService;
        private Services _services;
        private ProjectSettings _config;

        public UserController(AdminService service, AccountService accountService, Services services, CompanyService companyService, ProjectSettings config)
        {
            _service = service;
            _services = services;
            _accountService = accountService;
            _companyService = companyService;
            _config = config;
        }

        public virtual ActionResult Index()
        {
            return RedirectToAction("Users");
        }

        public virtual ActionResult Users()
        {
            var model = new UsersViewModel()
            {
                //Accounts = _service.GetUsers(),
                //Statistics = _service.GetRegistrationStatistics()
            };
            return View("Users", model);
        }

        public virtual IActionResult UsersQuery(IDataTablesRequest model)
        {
            var data = _service.Users_Query(model);
            var total = _service.Users_Total();

            var response = DataTablesResponse.Create(model, total, data.TotalCount, data.ToList());

            return new DataTablesJsonResult(response, true);
        }

        public JsonResult UsersJson(string term)
        {
            var users = _service.User_List(term, new ApiPaging(), null).Data;

            return Json(new { Data = users.ToSelectList(v => v.Id, t => t.Name) });
        }

        [HttpPost]
        public virtual RedirectResult Impersonate(int id, string redirectUrl = null)
        {
            var result = _accountService.ImpersonateAccount(id, this.LoginWithPrincipal);
            if (result.Success && String.IsNullOrEmpty(redirectUrl))
            {
                redirectUrl = Mvc.Root.Entry_ImpersonateRedirect.Url(this);
            }
            else
            {
                redirectUrl = Mvc.Root.Public_Index.Url(this);
            }
            return new RedirectResult(redirectUrl);
        }

        [HttpGet, AllowAnonymous]
        public virtual ActionResult UndoImpersonate(string redirectUrl = null)
        {
            var result = _accountService.RemoveImpersonation(this.LoginWithPrincipal);

            if (String.IsNullOrEmpty(redirectUrl))
            {
                redirectUrl = Url.Action("Users");
            }

            return new RedirectResult(redirectUrl);
        }

        public virtual ActionResult UsersDetails(int id)
        {
            var model = new UserDetailsModel { Account = _accountService.Get(id, visibleOnly: true) };

            model.LastActivities = _accountService.Activity_Last(id, 7);

            return View(model);
        }

        public virtual ActionResult ResetPassword(int id)
        {
            var user = _accountService.Get(id, visibleOnly: true);
            var result = _accountService.Password_SetRequestToken(user.Id);
            var model = new ResetPasswordViewModel { Id = id, Email = user.Email, Token = result.Result.ToShortGuid().ToString() };
            return PartialView(model);
        }

        [HttpPost, ActionName("ResetPassword")]
        public virtual ActionResult ResetPasswordPost(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = _accountService.Password_Reset(model, sendEmail: false);
                if (result.Success)
                {
                    model.Success = true;
                    return PartialView(model);
                }
                else
                {
                    result.AddToModelState(this);
                }
            }
            return ResetPassword(model.Id);
        }

        public virtual ActionResult ChangeDetails(int id)
        {
            var user = _accountService.GetForEdit(id).Result;
            return PartialView(user);
        }

        [HttpPost, ActionName("ChangeDetails")]
        public virtual ActionResult ChangeDetailsPost(int id, AccountDetailsEditModel model)
        {
            return this.ServiceCall(() => _accountService.Update(model, id, onBehalfOfUser: true))
                .OnSuccess(m =>
                {
                    return Mvc.Admin.User_Users_Details.Redirect(this, new { id });
                })
                .OnFailure(m => { return ChangeDetails(id); })
                .Call();
        }

        public virtual ActionResult ChangeRole(int id)
        {
            var user = _accountService.Get(id, visibleOnly: true);
            var model = new ChangeAccountRoleModel
            {
                Id = id,
                Role = user.UserRoles.FirstOrDefault()?.Role,
                RoleList = EnumHelper.GetValues<Role>().ToSelectList(v => v, t => t.GetDescription())
            };

            return PartialView(model);
        }

        [HttpPost, ActionName("ChangeRole")]
        public virtual ActionResult ChangeRolePost(ChangeAccountRoleModel model)
        {
            return this.ServiceCall(() => _accountService.ChangeAccountRoles(model))
                .OnSuccess(m =>
                {
                    return Mvc.Admin.User_Users_Details.Redirect(this, new { model.Id });
                })
                .OnFailure(m => { return ChangeRole(model.Id); })
                .Call();
        }

        public virtual ActionResult ChangeStatus(ChangeUserStatusModel model)
        {
            var user = _accountService.Get(model.Id, visibleOnly: true);
            model.Status = user.Status;
            return PartialView(model);
        }

        [HttpPost, ActionName("ChangeStatus")]
        public virtual ActionResult ChangeStatusPost(ChangeUserStatusModel model)
        {
            if (ModelState.IsValid)
            {
                _service.ChangeUserStatus(model);
                return Mvc.Admin.User_Users_Details.Redirect(this, new { id = model.Id });
            }
            return ChangeStatus(model);
        }

        public virtual ActionResult Statistics()
        {

            var model = _service.GetUsersStatistics();
            return View(model);

        }

        public virtual ActionResult Purge(int id)
        {
            var user = _accountService.Get(id, visibleOnly: true);
            return PartialView(user);
        }

        [HttpPost, ActionName("Purge")]
        public virtual ActionResult PurgePost(int id)
        {
            return this.ServiceCall(() => _accountService.Purge(id))
                .OnSuccess(m =>
                {
                    return Mvc.Admin.User_Users.Redirect(this);
                })
                .OnFailure(m => { return Purge(id); })
                .Call();
        }

        #region Activity

        public virtual ActionResult Activity()
        {
            return View(new UserActivityModel());
        }

        public virtual IActionResult ActivityQuery(IDataTablesRequest model, DateTime dateFrom, DateTime dateTo, EntityType? entityType, ActivityType? activityType)
        {
            var data = _service.GetActivities(model, dateFrom, dateTo, entityType, activityType);
            var total = _service.GetActivityTotal();

            var response = DataTablesResponse.Create(model, total, data.TotalCount, data.ToList());

            return new DataTablesJsonResult(response, true);
        }

        public virtual ActionResult ActivityDetail(int id)
        {
            var model = _service.GetActivity(id);
            return PartialView(model);
        }

        #endregion

        #region Invite
        public virtual ActionResult Invite()
        {
            var model = new InviteManageModel
            {
                Pending = _accountService.GetPendingInvites()
            };

            model.InviteRole = new InviteRoleViewModel()
            {
                //Change the default invite role if needed
                Role = Role.Administrator,
                CompanyList = _companyService.GetAll<CompanyViewModel>(includeDeleted: false).OrderBy(x => x.Name).ToList().ToSelectList(v => v.Id, t => t.Name)
            };

            model.RoleSelectionOptions = new List<SelectListItem>()
            {
                new SelectListItem() { Text = "Super admin", Value = Role.Administrator.ToString() },
                new SelectListItem() { Text = "Company admin", Value = Role.CompanyAdmin.ToString() }
            };

            return View("UsersInvite", model);
        }

        [HttpPost, ActionName("Invite")]
        public virtual ActionResult InvitePost([FromForm] InviteManageModel model)
        {
            return this.ServiceCall(() => _accountService.Invite(model, sendEmail: true))
                .OnSuccess(m =>
                {
                    TempData[PageMessage.Key()] = PageMessage.Success($"{model.FirstName} has been successfully invited.");
                    return Mvc.Admin.User_Invite.Redirect(this);
                })
                .OnFailure(() => Invite())
                .Call();
        }

        [HttpPost]
        public virtual ActionResult InviteResend(int id)
        {
            return this.ServiceCall(() => _accountService.Reinvite(id))
                .OnSuccess(m =>
                {
                    TempData[PageMessage.Key()] = PageMessage.Success($"{m.FirstName} has been successfully re-invited.");
                    return Mvc.Admin.User_Invite.Redirect(this);
                })
                .OnFailure(() => Invite())
                .Call();
        }

        public virtual ActionResult InviteUpload()
        {
            return PartialView("UsersInvite_Upload", new InviteUploadModel());
        }

        [HttpPost, ActionName("InviteUpload")]
        public virtual ActionResult InviteUploadPost(InviteUploadModel model)
        {
            return this.ServiceCall(() => _accountService.Invite_Upload(model))
                .OnSuccess(m =>
                {
                    TempData[PageMessage.Key()] = PageMessage.Success($"{m} users has been successfully invited.");
                    return Mvc.Admin.User_Invite.Redirect(this);
                })
                .OnFailure(() => InviteUpload())
                .Call();
        }

        public virtual ActionResult Import()
        {
            var model = new UserImportModel
            {
                CompanyList = _companyService.GetAll<CompanyViewModel>().ToSelectList(v => v.Id, t => t.Name)
            };
            return View("UsersImport", model);
        }

        [HttpPost, ActionName("Import")]
        public virtual ActionResult ImportPost(UserImportModel model)
        {
            return this.ServiceCall(() => _accountService.ImportUsers(model))
                .OnSuccess(m =>
                {
                    TempData[PageMessage.Key()] = PageMessage.Success($"{m.Invited} users have been successfully imported out of {m.Processed} rows");
                    TempData["UserImportResult"] = m.Path;
                    return Import();
                })
                .OnFailure(() => Import())
                .Call();
        }

        public virtual ActionResult ImportResult()
        {
            var path = TempData["UserImportResult"].ToString();
            var data = _accountService.ImportUsersResult(path);
            return File(data, "text/csv", $"{_config.ProjectDisplayName} users import result.csv".ToFilename());
        }

        #endregion

        #region Emails
        public virtual ActionResult Emails()
        {
            return this.ServiceCall(() => _accountService.Email_List())
            .Always(m =>
            {
                return View(m);
            })
            .Call();
        }

        [HttpPost]
        public IActionResult EmailsQuery(IDataTablesRequest model, DateTime dateFrom, DateTime dateTo)
        {
            var data = _accountService.Email_Search(model, dateFrom, dateTo);
            var count = _accountService.Email_Count();

            var response = DataTablesResponse.Create(model, count, data.TotalCount, data.ToList());
            return new DataTablesJsonResult(response, true);
        }

        public ActionResult EmailsDetail(int id)
        {
            return this.ServiceCall(() => _accountService.Email_Get(id))
                .Always(m =>
                {
                    return View(m);
                })
                .Call();
        }

        [HttpPost]
        public ActionResult EmailsResend(int id)
        {
            return this.ServiceCall(() => _accountService.Email_Resend(id))
                .Always(m =>
                {
                    TempData[PageMessage.Key()] = PageMessage.Success($"Email {m.TemplateId} is queued for resending");
                    return Mvc.Admin.User_Emails.Redirect(this);
                })
                .Call();
        }

        public ActionResult EmailsPreview()
        {
            return this.ServiceCall(() => _accountService.Email_Preview())
                .Always(m =>
                {
                    return View(m);
                })
                .Call();
        }

        public ActionResult EmailsPreviewShow(EmailPreviewModel model)
        {
            return this.ServiceCall(() => _accountService.Email_Preview(model))
                .Always(m =>
                {
                    return View("EmailsDetail", m);
                })
                .Call();
        }
        #endregion

        #region Sms

        public virtual ActionResult SmsList()
        {
            return View("SmsList");
        }

        [HttpPost]
        public IActionResult SmsQuery(IDataTablesRequest model, DateTime dateFrom, DateTime dateTo)
        {
            var data = _service.Sms_Search(model, dateFrom, dateTo);
            var count = _service.Sms_Count();

            var response = DataTablesResponse.Create(model, count, data.TotalCount, data.ToList());
            return new DataTablesJsonResult(response, true);
        }

        public ActionResult SmsDetail(int id)
        {
            return this.ServiceCall(() => _service.Sms_Get(id))
                .Always(m =>
                {
                    return View(m);
                })
                .Call();
        }

        #endregion

        #region Errors
        public ViewResult ErrorList()
        {
            return View("ErrorList", new ErrorListModel());
        }

        public IActionResult ErrorQuery(IDataTablesRequest model, DateTime dateFrom, DateTime dateTo, string search)
        {
            var data = _accountService.Error_Search(model, dateFrom, dateTo, search);
            var count = _accountService.Error_Count();

            var response = DataTablesResponse.Create(model, count, data.TotalCount, data.ToList());
            return new DataTablesJsonResult(response, true);
        }

        public ActionResult ErrorDetail(int id)
        {
            return this.ServiceCall(() => _accountService.Error_Get(id))
                .Always(m =>
                {
                    return View(m);
                })
                .Call();
        }
        #endregion

        #region Notifications
        public ViewResult NotificationList()
        {
            return View("NotificationList");
        }

        public IActionResult NotificationQuery(IDataTablesRequest model, DateTime dateFrom, DateTime dateTo)
        {
            var data = _accountService.PushNotification_Search(model, dateFrom, dateTo);
            var count = _accountService.PushNotification_Count();

            var response = DataTablesResponse.Create(model, count, data.TotalCount, data.ToList());
            return new DataTablesJsonResult(response, true);
        }

        public ActionResult NotificationDetail(int id)
        {
            return this.ServiceCall(() => _accountService.PushNotification_Get(id))
                .Always(m =>
                {
                    return View(m);
                })
                .Call();
        }

        #endregion

    }
}
