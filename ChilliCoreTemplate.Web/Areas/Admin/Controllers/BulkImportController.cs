using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Mvc;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Admin;
using ChilliCoreTemplate.Service.Admin;
using ChilliCoreTemplate.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using DataTables.AspNet.AspNetCore;
using DataTables.AspNet.Core;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Service.EmailAccount;
using System;
using System.Net.Mime;
using ChilliSource.Core.Extensions;

namespace ChilliCoreTemplate.Web.Areas.Admin.Controllers
{

    [Area("Admin")]
    [CustomAuthorize(Roles = AccountCommon.Administrator)]
    public class BulkImportController : Controller
    {
        private readonly BulkImportService _service;

        public BulkImportController(BulkImportService service)
        {
            _service = service;
        }

        public ActionResult Index()
        {
            return Mvc.Admin.BulkImport_List.Redirect(this);
        }

        public virtual ActionResult List()
        {
            return this.ServiceCall(_service.List).Call();
        }

        public ActionResult Download(int id)
        {
            return this.ServiceCall(() => _service.Download(id))
                .OnSuccess(m =>
                {
                    return new FileStreamResult(m, MediaTypeNames.Application.Zip)
                    {
                        FileDownloadName = $"BulkImport_{id}_{DateTime.UtcNow.ToTimezone().ToIsoDate()}.zip"
                    };
                })
                .Call();
        }
    }
}