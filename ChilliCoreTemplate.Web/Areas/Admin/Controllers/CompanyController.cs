using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service;
using ChilliCoreTemplate.Web.Controllers;
using ChilliSource.Cloud.Web.MVC;
using DataTables.AspNet.AspNetCore;
using DataTables.AspNet.Core;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ChilliCoreTemplate.Web.Areas.Admin.Controllers
{

    [Area("Admin")]
    [CustomAuthorize(Roles = AccountCommon.Administrator)]
    public class CompanyController : Controller
    {
        private Services _services;

        public CompanyController(Services services)
        {
            _services = services;
        }

        public ActionResult Index()
        {
            return Mvc.Admin.Company_List.Redirect(this);
        }

        public ActionResult List()
        {
            return View("CompanyList", new CompanyListModel());
        }

        [HttpPost]
        public IActionResult ListData(IDataTablesRequest model)
        {
            var data = _services.Company_List(model);
            var count = _services.Company_Count();

            return new DataTablesJsonResult(DataTablesResponse.Create(model, count, data.TotalCount, data), true);
        }

        public JsonResult ListJson(string term)
        {
            var companies = _services.Company_List(term, new ApiPaging(), null).Data;

            return Json(new { Data = companies.ToSelectList(v => v.Id, t => t.Name) });
        }

        public JsonResult Select2Query(string searchTerm, ApiPaging paging, int? id = null)
        {
            var companies = _services.Company_List(searchTerm, paging, id);

            return Json(new Select2QueryModel<DataLinkModel, object>(companies, x => new { id = x.Id, text = x.Name }));
        }

        public ActionResult Detail(int id)
        {
            return this.ServiceCall(() => _services.Company_Get(id))
                .Always(m => { return View("CompanyDetail", m); })
                .Call();
        }

        public ActionResult Edit(int? id = null, string name = null)
        {
            return this.ServiceCall(() => _services.Company_GetForEdit(id, name))
                .Always(m => { return View("CompanyEdit", m); })
                .Call();
        }

        [HttpPost]
        public ActionResult Edit([FromForm]CompanyEditModel model)
        {
            return this.ServiceCall(() => _services.Company_Edit(model))
                .OnSuccess(m =>
                {
                    TempData[PageMessage.Key()] = PageMessage.Success("Company {0} has been successfully {1}.".FormatWith(m.Name, model.Id == 0 ? "created" : "saved"));
                    return Mvc.Admin.Company_Detail.Redirect(this, m.Id);
                })
                .OnFailure(() => Edit(model.Id))
                .Call();
        }

        public ActionResult Delete(int id)
        {
            return this.ServiceCall(() => _services.Company_Get(id))
                .Always(m =>
                {
                    return PartialView("CompanyDelete", m);
                })
                .Call();
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeletePost(int id)
        {
            return this.ServiceCall(() => _services.Company_Delete(id))
                .OnSuccess(m =>
                {
                    TempData[PageMessage.Key()] = PageMessage.Success($"The company {m.Name} was {(m.IsDeleted ? "deleted" : "unarchived")}");
                    return Mvc.Admin.Company_List.Redirect(this);
                })
                .OnFailure(() => Delete(id))
                .Call();
        }

        public ActionResult Purge(int id)
        {
            return this.ServiceCall(() => _services.Company_Get(id))
                .Always(m =>
                {
                    return PartialView("CompanyPurge", m);
                })
                .Call();
        }

        [HttpPost, ActionName("Purge")]
        public ActionResult PurgePost(int id)
        {
            return this.ServiceCall(() => _services.Company_Purge(id))
                .OnSuccess(m =>
                {
                    TempData[PageMessage.Key()] = PageMessage.Success($"The company {m.Name} was purged");
                    return Mvc.Admin.Company_List.Redirect(this);
                })
                .OnFailure(() => Delete(id))
                .Call();
        }

        [HttpPost]
        public IActionResult AdminList(IDataTablesRequest model, int id)
        {
            var data = _services.Company_Admin_List(model, id, Role.CompanyAdmin);
            var count = _services.Company_Admin_Count(id, Role.CompanyAdmin);

            return new DataTablesJsonResult(DataTablesResponse.Create(model, count, data.TotalCount, data.ToList()), true);
        }

        [HttpPost]
        public ActionResult AdminAdd([FromForm] CompanyDetailViewModel model)
        {
            model.Admin.InviteRole = new InviteRoleViewModel { CompanyId = model.Id, Role = Role.CompanyAdmin };
            return this.ServiceCall(() => _services.Company_Admin_Add(model.Id, model.Admin))
                .OnSuccess(m =>
                {
                    ModelState.Clear();
                    return PartialView("CompanyAdminAdd", m);
                })
                .OnFailure(() =>
                {
                    model = _services.Company_Get(model.Id).Result;
                    return PartialView("CompanyAdminAdd", model);
                })
                .Call();
        }

        public ActionResult AdminRemove(int id, int userId)
        {
            return this.ServiceCall(() => _services.Company_Admin_Get(id, userId))
                .Always(m =>
                {
                    return PartialView("CompanyAdminDelete", m);
                })
                .Call();
        }

        [HttpPost, ActionName("AdminRemove")]
        public ActionResult AdminRemovePost(int id, int userId)
        {
            return this.ServiceCall(() => _services.Company_Admin_Delete(id, userId))
                .OnSuccess(() => { return Ok(); })
                .OnFailure(() => AdminRemove(id, userId))
                .Call();
        }

        public JsonResult AdminDetail(int id, string email)
        {
            var workerDetails = _services.Company_Admin_Details(id, email, Role.CompanyAdmin);
            return Json(workerDetails);
        }


        [HttpPost]
        public ActionResult Impersonate(int id)
        {
            return this.ServiceCall(() => _services.Company_Impersonate(id, this.LoginWithPrincipal))
                .OnSuccess(m =>
                {
                    return Mvc.Root.Entry_ImpersonateRedirect.Redirect(this);
                })
                .OnFailure(() =>
                {
                    TempData[PageMessage.Key()] = PageMessage.Warning(ModelState.Errors().First().Value.First());
                    return Mvc.Admin.Company_List.Redirect(this);
                })
                .Call();

        }

    }
}