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

        public ActionResult Settings()
        {
            return this.ServiceCall(() => _services.Company_GetSettings()).Call();
        }

        [HttpPost]
        public ActionResult Settings([FromForm] CompanySettingsModel model)
        {
            return this.ServiceCall(() => _services.Company_SaveSettings(model))
                .OnSuccess(m =>
                {
                    TempData[PageMessage.Key()] = PageMessage.Success("Settings has been successfully saved");
                    return Mvc.Company.Default.Redirect(this);
                })
                .OnFailure(() => Settings())
                .Call();
        }

    }
}