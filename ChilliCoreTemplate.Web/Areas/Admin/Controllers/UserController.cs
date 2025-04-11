using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Admin;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Models.Sms;
using ChilliCoreTemplate.Service.Admin;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliCoreTemplate.Web.Controllers;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web.MVC;
using ChilliSource.Core.Extensions;
using DataTables.AspNet.AspNetCore;
using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChilliCoreTemplate.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [CustomAuthorize(Roles = AccountCommon.Administrator)]
    public class UserController : Controller
    {
        private AdminService _service;
        private CompanyService _companyService;
        private AccountService _accountService;
        private ProjectSettings _config;

        public UserController(AdminService service, AccountService accountService, CompanyService companyService, ProjectSettings config)
        {
            _service = service;
            _accountService = accountService;
            _companyService = companyService;
            _config = config;
        }

        public virtual ActionResult Index()
        {
            return RedirectToAction("Users");
        }

        public virtual ActionResult Users(UserListModel model)
        {
            return this.ServiceCall(() => _service.User_List(model))
                .OnSuccess(m =>
                {
                    return View("UsersList", m);
                })
                .Call();
        }

        public virtual IActionResult UsersQuery(IDataTablesRequest model)
        {
            var data = _service.Users_Query(model);
            var total = _service.Users_Total();

            var response = DataTablesResponse<UserSummaryViewModel>.Create(model, total, data.TotalCount, data);

            return new JsonResult(response);
        }

        public JsonResult UsersJson(string term, Role? role = null)
        {
            var users = _service.User_List(term, new ApiPaging(), null, role).Data;

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
            var model = new UserDetailsModel { Account = _accountService.Get<AccountViewModel>(id, visibleOnly: true) };

            model.LastActivities = _accountService.Activity_Last(id, 7);

            return View(model);
        }

        public virtual ActionResult ResetPassword(int userId)
        {
            var user = _accountService.Get<AccountViewModel>(userId, visibleOnly: true);
            var result = _accountService.Password_SetRequestToken(user.Id);
            var model = new ResetPasswordViewModel { UserId = userId, Email = user.Email, Token = result.Result.ToShortGuid().ToString() };
            return PartialView(model);
        }

        [HttpPost, ActionName("ResetPassword")]
        public virtual ActionResult ResetPasswordPost(ResetPasswordViewModel model)
        {
            return this.ServiceCall(() => _accountService.Password_Reset(model, sendEmail: false))
                .OnSuccess(m =>
                {
                    TempData[PageMessage.Key()] = PageMessage.Success($"Password was successfully reset.");
                    return Mvc.Admin.User_Users_Details.Redirect(this, m);
                })
                .OnFailure(m => ResetPassword(model.UserId))
                .Call();
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
            var user = _accountService.Get<AccountViewModel>(id, visibleOnly: true);
            var model = new ChangeAccountRoleModel
            {
                Id = id,
                CurrentRoles = user.UserRoles
            };
            return PartialView("ChangeRole", model);
        }

        public virtual ActionResult RemoveRole(UserRemoveRoleModel model)
        {
            return PartialView("RemoveRole", model);
        }

        [HttpPost, ActionName("RemoveRole")]
        public virtual ActionResult RemoveRolePost(UserRemoveRoleModel model)
        {
            return this.ServiceCall(() => _accountService.AccountRoles_Remove(model))
                .OnSuccess(m => { return Ok(); })
                .OnFailure(m => { return RemoveRole(model); })
                .Call();
        }

        public virtual ActionResult AddRole(int id)
        {
            var model = new UserAddRoleModel
            {
                Id = id,
                RoleList = EnumHelper.GetValues<Role>().ToSelectList(v => v, t => t.GetDescription()),
            };
            return PartialView("AddRole", model);
        }

        [HttpPost, ActionName("AddRole")]
        public virtual ActionResult AddRolePost(UserAddRoleModel model)
        {
            return this.ServiceCall(() => _accountService.AccountRoles_Add(model))
                .OnSuccess(() => { return Ok(); })
                .OnFailure(m => { return AddRole(model.Id); })
                .Call();
        }

        public virtual ActionResult ChangeStatus(ChangeUserStatusModel model)
        {
            var user = _accountService.Get<AccountViewModel>(model.Id, visibleOnly: true);
            model.Status = user.Status;
            model.IsInvited = user.Status == UserStatus.Registered && user.UserRoles.Any(x => x.Status == RoleStatus.Invited);
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
            var user = _accountService.Get<AccountViewModel>(id, visibleOnly: true);
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

        public virtual ActionResult Export()
        {
            var model = new UsersExportModel
            {
                RoleList = EnumHelper.GetValues<Role>().ToSelectList(v => v, t => t.GetDescription())
            };

            return PartialView("UsersExport", model);
        }

        [HttpPost, ActionName("Export")]
        public virtual ActionResult ExportPost(UsersExportModel model)
        {
            return this.ServiceCall(() => _service.Users_Export(model))
                .OnSuccess(m =>
                {
                    return new FileContentResult(m.ToByteArray(), MyMediaTypeNames.Text.Csv)
                    {
                        FileDownloadName = $"UserExport_{DateTime.UtcNow.ToTimezone().ToIsoDate()}.csv"
                    };
                })
                .OnFailure(m => { return Export(); })
                .Call();
        }

        #region Activity

        public virtual ActionResult Activity(int? userId, EntityType? entityType)
        {
            return View(new UserActivityModel { UserId = userId, Entity = entityType });
        }

        public virtual IActionResult ActivityQuery(IDataTablesRequest model, DateTime dateFrom, DateTime dateTo, EntityType? entityType, ActivityType? activityType, int? userId)
        {
            var data = _service.GetActivities(model, dateFrom, dateTo, entityType, activityType, userId);
            var total = _service.GetActivityTotal();

            return model.GetActionResult(total, data.TotalCount, data);
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
                CompanyList = _companyService.List<CompanyViewModel>(includeDeleted: false).OrderBy(x => x.Name).ToList().ToSelectList(v => v.Id, t => t.Name)
            };

            model.RoleSelectionOptions = new List<SelectListItem>()
            {
                new SelectListItem() { Text = "Super admin", Value = Role.Administrator.ToString() },
                new SelectListItem() { Text = Role.CompanyAdmin.GetDescription(), Value = Role.CompanyAdmin.ToString() },
                new SelectListItem() { Text = Role.CompanyUser.GetDescription(), Value = Role.CompanyUser.ToString() }
            };

            return View("UsersInvite", model);
        }

        [HttpPost, ActionName("Invite")]
        public virtual ActionResult InvitePost([FromForm] InviteEditModel model)
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

        public ActionResult InviteResend(int id)
        {
            return this.ServiceCall(() => _companyService.Company_Admin_Get(id)).Call();
        }

        [HttpPost, ActionName("InviteResend")]
        public virtual ActionResult InviteResendPost(int id)
        {
            return this.ServiceCall(() => _accountService.Invite_Resend(id))
                .OnSuccess(m =>
                {
                    TempData[PageMessage.Key()] = PageMessage.Success($"{m.FirstName} has been successfully re-invited.");
                    return Mvc.Admin.User_Invite.Redirect(this);
                })
                .OnFailure(() => InviteResend(id))
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

        //public virtual ActionResult Import()
        //{
        //    var model = new UserImportModel
        //    {
        //        CompanyList = _companyService.List<CompanyViewModel>().ToSelectList(v => v.Id, t => t.Name)
        //    };
        //    return View("UsersImport", model);
        //}

        //[HttpPost, ActionName("Import")]
        //public virtual ActionResult ImportPost(UserImportModel model)
        //{
        //    return this.ServiceCall(() => _accountService.ImportUsers(model))
        //        .OnSuccess(m =>
        //        {
        //            TempData[PageMessage.Key()] = PageMessage.Success($"{m.Invited} users have been successfully imported out of {m.Processed} rows");
        //            TempData["UserImportResult"] = m.Path;
        //            return Import();
        //        })
        //        .OnFailure(() => Import())
        //        .Call();
        //}

        //public virtual ActionResult ImportResult()
        //{
        //    var path = TempData["UserImportResult"].ToString();
        //    var data = _accountService.ImportUsersResult(path);
        //    return File(data, "text/csv", $"{_config.ProjectDisplayName} users import result.csv".ToFilename());
        //}

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

            return model.GetActionResult(count, data.TotalCount, data);
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

            return model.GetActionResult(count, data.TotalCount, data);
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
            return View("NotificationList", new PushNotificationListModel());
        }

        public IActionResult NotificationQuery(IDataTablesRequest model, DateTime dateFrom, DateTime dateTo)
        {
            var data = _accountService.PushNotification_Search(model, dateFrom, dateTo);
            var count = _accountService.PushNotification_Count();

            return model.GetActionResult(count, data.TotalCount, data);
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
