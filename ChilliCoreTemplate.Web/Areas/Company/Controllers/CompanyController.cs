using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Service;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliSource.Cloud.Web.MVC;
using Humanizer;
using Microsoft.AspNetCore.Mvc;

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
            return Mvc.Company.Company_Detail.Redirect(this);
        }

        public ActionResult Detail()
        {
            return this.ServiceCall(() => _service.Get())
                .Always(m => { return View("CompanyDetail", m); })
                .Call();
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

    }
}