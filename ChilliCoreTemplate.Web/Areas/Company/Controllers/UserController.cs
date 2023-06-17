using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Admin;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service;
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
using System.Collections.Generic;
using System.Linq;

namespace ChilliCoreTemplate.Web.Areas.Company.Controllers
{
    [Area("Company")]
    [CustomAuthorize(Roles = AccountCommon.CompanyAdmin)]
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

        public virtual ActionResult List()
        {
            var model = new UserListModel();
            model.RoleList = model.RoleList.Where(x => x.Value != Role.Administrator.ToString()).ToSelectList();
            return View("UserList", model);
        }

        public virtual IActionResult ListData(IDataTablesRequest model)
        {
            var data = _service.Users_Query(model);
            var total = _service.Users_Total();

            var response = DataTablesResponse.Create(model, total, data.TotalCount, data.ToList());

            return new DataTablesJsonResult(response, true);
        }

        //[HttpPost]
        //public virtual RedirectResult Impersonate(int id, string redirectUrl = null)
        //{
        //    var result = _accountService.ImpersonateAccount(id, this.LoginWithPrincipal);
        //    if (result.Success)
        //    {
        //        redirectUrl = Mvc.Company.Location_List.Url(this);
        //    }
        //    else
        //    {
        //        redirectUrl = Mvc.Root.Public_Index.Url(this);
        //    }
        //    return new RedirectResult(redirectUrl);
        //}

        [HttpGet, AllowAnonymous]
        public virtual ActionResult UndoImpersonate(string redirectUrl = null)
        {
            var result = _accountService.RemoveImpersonation(this.LoginWithPrincipal);

            if (result.Success)
            {
                redirectUrl = (result.Result.UserData().CompanyId == null) ? Mvc.Admin.Company_List.Url(this) : Mvc.Company.User_List.Url(this);
            }
            else if (redirectUrl == null)
            {
                redirectUrl = Mvc.Company.User_List.Url(this);
            }

            return new RedirectResult(redirectUrl);
        }

        public virtual ActionResult Detail(int id)
        {
            var model = new UserDetailsModel { Account = _accountService.Get(id, visibleOnly: true) };

            //model.LastActivities = _accountService.Activity_Last(id, 7);

            return View("UserDetail", model);
        }

        public virtual ActionResult ResetPassword(int userId)
        {
            var user = _accountService.Get(userId, visibleOnly: true);
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
                .OnFailure(ResetPassword)
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
                    return Mvc.Company.User_Detail.Redirect(this, new { id });
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
                Roles = user.UserRoles.Select(x => x.Role).ToList(),
                RoleList = EnumHelper.GetValues<Role>().ToSelectList(v => v, t => t.GetDescription())
            };

            return PartialView(model);
        }

        [HttpPost, ActionName("ChangeRole")]
        public virtual ActionResult ChangeRolePost([FromForm] ChangeAccountRoleModel model)
        {
            return this.ServiceCall(() => _accountService.ChangeAccountRoles(model))
                .OnSuccess(m =>
                {
                    TempData[PageMessage.Key()] = PageMessage.Success($"Roles successfully updated.");
                    return Mvc.Company.User_Detail.Redirect(this, new { model.Id });
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
                return Mvc.Company.User_Detail.Redirect(this, new { id = model.Id });
            }
            return ChangeStatus(model);
        }

        //public virtual ActionResult Statistics()
        //{

        //    var model = _service.GetUsersStatistics();
        //    return View(model);

        //}


        //#region Activity

        //public virtual ActionResult Activity()
        //{
        //    return View();
        //}

        //public virtual IActionResult ActivityQuery(IDataTablesRequest model, DateTime dateFrom, DateTime dateTo, EntityType? entityType, ActivityType? activityType)
        //{
        //    var data = _service.GetActivities(model, dateFrom, dateTo, entityType, activityType);
        //    var total = _service.GetActivityTotal();

        //    var response = DataTablesResponse.Create(model, total, data.TotalCount, data.ToList());

        //    return new DataTablesJsonResult(response, true);
        //}

        //public virtual ActionResult ActivityDetail(int id)
        //{
        //    var model = _service.GetActivity(id);
        //    return PartialView(model);
        //}

        //#endregion

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
                Role = Role.CompanyAdmin,
                CompanyId = User.UserData().CompanyId
            };

            model.RoleSelectionOptions = new List<SelectListItem>()
            {
                new SelectListItem() { Text = "Company admin", Value = Role.CompanyAdmin.ToString() }
            };

            return View("UserInvite", model);
        }

        [HttpPost, ActionName("Invite")]
        public virtual ActionResult InvitePost([FromForm] InviteManageModel model)
        {
            return this.ServiceCall(() => _accountService.Invite(model, sendEmail: true))
                .OnSuccess(m =>
                {
                    TempData[PageMessage.Key()] = PageMessage.Success($"{model.FirstName} has been successfully invited.");
                    return Mvc.Company.User_Invite.Redirect(this);
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
                    return Mvc.Company.User_Invite.Redirect(this);
                })
                .OnFailure(() => Invite())
                .Call();
        }

        //public virtual ActionResult InviteUpload()
        //{
        //    return PartialView("UsersInvite_Upload", new InviteUploadModel());
        //}

        //[HttpPost, ActionName("InviteUpload")]
        //public virtual ActionResult InviteUploadPost(InviteUploadModel model)
        //{
        //    return this.ServiceCall(() => _accountService.Invite_Upload(model))
        //        .OnSuccess(m =>
        //        {
        //            TempData[PageMessage.Key()] = PageMessage.Success($"{m} users has been successfully invited.";
        //            return Mvc.Root.User_Invite.Redirect(this);
        //        })
        //        .OnFailure(() => InviteUpload())
        //        .Call();
        //}

        //public virtual ActionResult Import()
        //{
        //    var model = new UserImportModel
        //    {
        //        CompanyList = _companyService.GetAll<CompanyViewModel>().ToSelectList(v => v.Id, t => t.Name)
        //    };
        //    return View("UsersImport", model);
        //}

        //[HttpPost, ActionName("Import")]
        //public virtual ActionResult ImportPost(UserImportModel model)
        //{
        //    return this.ServiceCall(() => _accountService.ImportUsers(model))
        //        .OnSuccess(m =>
        //        {
        //            TempData[PageMessage.Key()] = PageMessage.Success($"{m.Invited} users have been successfully imported out of {m.Processed} rows";
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


        //#region Emails
        //public virtual ActionResult Emails()
        //{
        //    return View("Emails");
        //}

        ////TODO: Emails
        ////public IActionResult EmailsQuery(IDataTablesRequest model, DateTime dateFrom, DateTime dateTo)
        ////{
        ////    var data = _accountService.Email_Search(model, dateFrom, dateTo);
        ////    var count = _accountService.Email_Count();

        ////    var response = DataTablesResponse.Create(model, count, data.TotalCount, data.ToList());
        ////    return new DataTablesJsonResult(response, true);
        ////}

        ////public ActionResult EmailsDetail(int id)
        ////{
        ////    return this.ServiceCall(() => _accountService.Email_Get(id))
        ////        .Always(m =>
        ////        {
        ////            return View(m);
        ////        })
        ////        .Call();
        ////}

        ////[HttpPost]
        ////public ActionResult EmailsResend(int id)
        ////{
        ////    return this.ServiceCall(() => _accountService.Email_Resend(id))
        ////        .Always(m =>
        ////        {
        ////            TempData[PageMessage.Key()] = PageMessage.Success( $"Email {m.TemplateId} is queued for resending";
        ////            return Menu.Admin_User_Emails.Redirect();
        ////        })
        ////        .Call();
        ////}

        ////public ActionResult EmailsPreview()
        ////{
        ////    return this.ServiceCall(() => _accountService.Email_Preview())
        ////        .Always(m =>
        ////        {
        ////            return View(m);
        ////        })
        ////        .Call();
        ////}

        ////public ActionResult EmailsPreviewShow(EmailPreviewModel model)
        ////{
        ////    return this.ServiceCall(() => _accountService.Email_Preview(model))
        ////        .Always(m =>
        ////        {
        ////            return View("EmailsDetail", m);
        ////        })
        ////        .Call();
        ////}
        //#endregion


    }
}
