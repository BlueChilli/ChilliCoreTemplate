using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliCoreTemplate.Web.Controllers;
using ChilliSource.Cloud.Web.MVC;
using DataTables.AspNet.AspNetCore;
using DataTables.AspNet.Core;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ChilliCoreTemplate.Web.Areas.Company.Controllers
{

    [Area("Company")]
    [CustomAuthorize(Roles = AccountCommon.CompanyAdmin)]
    public class CompanyController : Controller
    {
        private CompanyService _service;

        public CompanyController(CompanyService service)
        {
            _service = service;
        }

        public ActionResult Index()
        {
            return Mvc.Company.Company_List.Redirect(this);
        }

        public ActionResult Settings()
        {
            return this.ServiceCall(() => _service.GetSettings()).Call();
        }

        [HttpPost]
        public ActionResult Settings([FromForm] CompanySettingsModel model)
        {
            return this.ServiceCall(() => _service.SaveSettings(model))
                .OnSuccess(m =>
                {
                    TempData[PageMessage.Key()] = PageMessage.Success("Settings has been successfully saved");
                    return Mvc.Company.Default.Redirect(this);
                })
                .OnFailure(() => Settings())
                .Call();
        }

        public ActionResult List()
        {
            var model = new CompanyListModel()
            {
                List = _service.GetAll<CompanyDetailViewModel>().Where(x => x.Id != User.UserData().CompanyId).ToList()
            };

            return View("CompanyList", model);
        }

        public ActionResult Edit(int? id = null)
        {
            return this.ServiceCall(() => _service.GetForEdit(id))
                .Call();
        }

        [HttpPost]
        public ActionResult Edit([FromForm] CompanyEditModel model)
        {
            model.MasterCompanyId = User.UserData().CompanyId;
            return this.ServiceCall(() => _service.Edit(model))
                .OnSuccess(m =>
                {
                    TempData[PageMessage.Key()] = PageMessage.Success("Company {0} has been successfully {1}.".FormatWith(m.Name, model.Id == 0 ? "created" : "saved"));
                    return Mvc.Company.Company_Edit.Redirect(this, m.Id);
                })
                .OnFailure(() => Edit(model.Id))
                .Call();
        }

        [HttpPost]
        public IActionResult AdminList(IDataTablesRequest model, int id)
        {
            var data = _service.Company_Admin_List(model, id, Role.CompanyAdmin);
            var count = _service.Company_Admin_Count(id, Role.CompanyAdmin);

            return new DataTablesJsonResult(DataTablesResponse.Create(model, count, data.TotalCount, data.ToList()), true);
        }

        [HttpPost]
        public ActionResult AdminAdd([FromForm] CompanyAdminAddModel model)
        {
            model.Admin.InviteRole = new InviteRoleViewModel { CompanyId = model.Id, Role = Role.CompanyAdmin };
            return this.ServiceCall(() => _service.Company_Admin_Add(model.Id, model.Admin))
                .OnSuccess(m =>
                {
                    ModelState.Clear();
                    return PartialView("CompanyAdminAdd", new CompanyEditModel());
                })
                .OnFailure(() =>
                {
                    return PartialView("CompanyAdminAdd", _service.GetForEdit(model.Id).Result);
                })
                .Call();
        }

        public ActionResult AdminRemove(int id, int userId)
        {
            return this.ServiceCall(() => _service.Company_Admin_Get(userId))
                .Always(m =>
                {
                    return PartialView("CompanyAdminDelete", m);
                })
                .Call();
        }

        [HttpPost, ActionName("AdminRemove")]
        public ActionResult AdminRemovePost(int id, int userId)
        {
            return this.ServiceCall(() => _service.Company_Admin_Delete(id, userId))
                .OnSuccess(() => { return Ok(); })
                .OnFailure(() => AdminRemove(id, userId))
                .Call();
        }

        public JsonResult AdminDetail(int id, string email)
        {
            var workerDetails = _service.Company_Admin_Details(id, email, Role.CompanyAdmin);
            return Json(workerDetails);
        }

        public ActionResult Impersonate(int id)
        {
            return this.ServiceCall(() => _service.Company_Impersonate(id, this.LoginWithPrincipal))
                .OnSuccess(m =>
                {
                    return new RedirectResult(Mvc.Company.User_List.Url(this));
                })
                .OnFailure(() =>
                {
                    TempData[PageMessage.Key()] = PageMessage.Warning(ModelState.Errors().First().Value.First());
                    return Mvc.Company.Company_List.Redirect(this);
                })
                .Call();

        }

    }
}