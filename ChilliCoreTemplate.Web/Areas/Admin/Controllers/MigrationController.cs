using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Admin.Migration;
using ChilliCoreTemplate.Service.Admin;
using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ChilliCoreTemplate.Web.Areas.Admin.Controllers
{

    [Area("Admin")]
    [CustomAuthorize(Roles = AccountCommon.Administrator)]
    public class MigrationController : Controller
    {
        private MigrationService _service;

        public MigrationController(MigrationService service)
        {
            _service = service;
        }

        public ActionResult Import()
        {
            return View("MigrationImport", new MigrationImportModel());
        }

        [HttpPost]
        public ActionResult Import(MigrationImportModel model)
        {
            return this.ServiceCall(() => _service.Migration(model))
                .OnSuccess(m =>
                {
                    TempData[PageMessage.Key()] = PageMessage.Success("Migration has run");
                    return Mvc.Admin.Migration_Import.Redirect(this);
                })
                .OnFailure(() => Import())
                .Call();
        }
    }
}