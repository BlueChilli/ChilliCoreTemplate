using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Service;
using ChilliSource.Cloud.Web.MVC;
using Humanizer;
using Microsoft.AspNetCore.Mvc;

namespace ChilliCoreTemplate.Web.Areas.Company.Controllers
{

    [Area("Company")]
    [CustomAuthorize(Roles = AccountCommon.CompanyAdmin)]
    public class CompanyController : Controller
    {
        private Services _services;

        public CompanyController(Services services)
        {
            _services = services;
        }

        public ActionResult Index()
        {
            return Mvc.Company.Company_Detail.Redirect(this);
        }

        public ActionResult Detail()
        {
            return this.ServiceCall(() => _services.Company_Get())
                .Always(m => { return View("CompanyDetail", m); })
                .Call();
        }

        public ActionResult Edit()
        {
            return this.ServiceCall(() => _services.Company_GetForEdit())
                .Always(m => { return View("CompanyEdit", m); })
                .Call();
        }

        [HttpPost]
        public ActionResult Edit([FromForm]CompanyEditModel model)
        {
            model.MasterCompanyId = User.UserData().CompanyId;
            return this.ServiceCall(() => _services.Company_Edit(model))
                .OnSuccess(m =>
                {
                    TempData[PageMessage.Key()] = PageMessage.Success("Company {0} has been successfully {1}.".FormatWith(m.Name, model.Id == 0 ? "created" : "saved"));
                    return Mvc.Company.Company_Detail.Redirect(this);
                })
                .OnFailure(() => Edit())
                .Call();
        }

    }
}