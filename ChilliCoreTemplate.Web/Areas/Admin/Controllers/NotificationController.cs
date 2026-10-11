using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web.MVC;
using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;

namespace ChilliCoreTemplate.Web.Areas.Admin.Controllers;

[Area("Admin")]
[CustomAuthorize(Roles = AccountCommon.Administrator)]
public class NotificationController(PushNotificationService service) : Controller
{
    private readonly PushNotificationService _service = service;

    public ViewResult List(int? userId)
    {
        return View("NotificationList", new PushNotificationListModel { UserId = userId });
    }

    public IActionResult ListData(IDataTablesRequest model, PushNotificationListModel filterModel)
        => model.GetActionResult(_service.List(model, filterModel));

    public ActionResult Detail(int id)
    {
        return this.ServiceCall(() => _service.Get(id))
            .Always(m =>
            {
                return View(m);
            })
            .Call();
    }

}
