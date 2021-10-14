using ChilliSource.Cloud.Web;
using ChilliSource.Cloud.Web.MVC;
using DataTables.AspNet.AspNetCore;
using DataTables.AspNet.Core;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service;
using ChilliCoreTemplate.Web;
using System.Collections.Generic;
using System.Linq;

namespace ChilliCoreTemplate.Web.Areas.Company.Controllers
{

    [Area("Company")]
    [RequireHttpsWeb, CustomAuthorize(MultipleRoles = new string[] { AccountCommon.CompanyAdmin, AccountCommon.CompanyUser })]
    public class LocationController : Controller
    {
        private Services _services;

        public LocationController(Services services)
        {
            _services = services;
        }

        public ActionResult Index()
        {
            return Mvc.Company.Location_List.Redirect(this);
        }

        public ActionResult List()
        {
            return this.ServiceCall(() => _services.Location_List())
                .Always(m => { return View("LocationList", m); })
                .Call();
        }

        [CustomAuthorize(MultipleRoles = new string[] { AccountCommon.CompanyAdmin })]
        public ActionResult Edit(int? id = null)
        {
            return this.ServiceCall(() => _services.Location_GetForEdit(id))
                .Always(m => { return View("LocationEdit", m); })
                .Call();
        }

        [CustomAuthorize(MultipleRoles = new string[] { AccountCommon.CompanyAdmin })]
        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(LocationEditModel model)
        {
            return this.ServiceCall(() => _services.Location_Edit(model))
                .OnSuccess(m =>
                {
                    TempData[PageMessage.Key()] = PageMessage.Success("Location {0} has been successfully {1}.".FormatWith(m.Name, model.Id == 0 ? "created" : "saved"));
                    return Mvc.Company.Location_Detail.Redirect(this, m.Id);
                })
                .OnFailure(() => Edit(model.Id))
                .Call();
        }

        public ActionResult Detail(int id)
        {
            return this.ServiceCall(() => _services.Location_GetDetail(id))
                .Always(m =>
                {
                    return View("LocationDetail", m);
                })
                .Call();
        }

        [CustomAuthorize(Roles = AccountCommon.CompanyAdmin)]
        public ActionResult Delete(int id)
        {
            return this.ServiceCall(() => _services.Location_Get(id))
                .Always(m =>
                {
                    return PartialView("LocationDelete", m);
                })
                .Call();
        }

        [CustomAuthorize(Roles = AccountCommon.CompanyAdmin)]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeletePost(int id)
        {
            return this.ServiceCall(() => _services.Location_Delete(id))
                .OnSuccess(m =>
                {
                    TempData[PageMessage.Key()] = PageMessage.Success( $"The site {m.Name} was deleted");
                    return Mvc.Company.Location_List.Redirect(this);
                })
                .OnFailure(() => Delete(id))
                .Call();
        }

        [HttpPost]
        public IActionResult UserList(IDataTablesRequest model, int id)
        {
            var data = _services.Location_User_List(model, id, Role.CompanyUser);
            var count = _services.Location_User_Count(id, Role.CompanyUser);

            return new DataTablesJsonResult(DataTablesResponse.Create(model, count, data.TotalCount, data.ToList()), true);
        }

        public ActionResult UserAdd(LocationDetailModel model)
        {
            model.User.InviteRole = new InviteRoleViewModel { CompanyId = User.UserData().CompanyId, Role = Role.CompanyUser };
            model.User.LocationIds = new List<int> { model.Id };
            return this.ServiceCall(() => _services.Location_User_Add(model.Id, model.User))
                .OnSuccess(m =>
                {
                    ModelState.Clear();
                    return PartialView("LocationUserAdd", m);
                })
                .OnFailure(() =>
                {
                    model = _services.Location_GetDetail(model.Id).Result;
                    return PartialView("LocationUserAdd", model);
                })
                .Call();
        }

        public ActionResult UserDelete(int id, int userId)
        {
            return this.ServiceCall(() => _services.Location_User_Get(id, userId))
                .Always(m =>
                {
                    return PartialView("LocationUserDelete", m);
                })
                .Call();
        }

        [HttpPost, ActionName("UserDelete")]
        public ActionResult UserDeletePost(int id, int userId)
        {
            return this.ServiceCall(() => _services.Location_User_Delete(id, userId))
                 .OnSuccess(m =>
                 {
                     return new EmptyResult();
                 })
                .OnFailure(() => UserDelete(id, userId))
                .Call();
        }

        public JsonResult UserDetail(string email)
        {
            var workerDetails = _services.Location_User_Details(email, Role.CompanyUser);
            return Json(workerDetails);
        }

    }
}