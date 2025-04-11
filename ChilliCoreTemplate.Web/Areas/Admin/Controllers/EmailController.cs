using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Admin;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Models.Sms;
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
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChilliCoreTemplate.Web.Areas.Admin.Controllers;

[Area("Admin")]
[CustomAuthorize(Roles = AccountCommon.Administrator)]
public class EmailController(EmailService service) : Controller
{
    private readonly EmailService _service = service;

    public virtual ActionResult Index()
    {
        return Mvc.Admin.Email_List.Redirect(this);
    }

    public virtual ActionResult List(int? userId)
    {
        return this.ServiceCall(() => _service.List(userId))
        .Call();
    }

    [HttpPost]
    public IActionResult ListData(IDataTablesRequest model, DateTime dateFrom, DateTime dateTo, int? userId)
    {
        var data = _service.Search(model, dateFrom, dateTo, userId);
        var count = _service.Count();

        var response = DataTablesResponse<EmailSummaryModel>.Create(model, count, data.TotalCount, data.ToList());
        return new JsonResult(response);
    }

    public ActionResult Detail(int id)
    {
        return this.ServiceCall(() => _service.Email_Get(id))
            .Call();
    }

    [HttpPost]
    public ActionResult Resend(int id)
    {
        return this.ServiceCall(() => _service.Email_Resend(id))
            .Always(m =>
            {
                TempData[PageMessage.Key()] = PageMessage.Success($"Email {m.TemplateId} is queued for resending");
                return Mvc.Admin.Email_List.Redirect(this);
            })
            .Call();
    }

    public ActionResult Preview()
    {
        return this.ServiceCall(() => _service.Email_Preview())
            .Call();
    }

    public ActionResult PreviewShow(EmailPreviewModel model)
    {
        return this.ServiceCall(() => _service.Email_Preview(model))
            .OnSuccess(m =>
            {
                return View("EmailDetail", m);
            })
            .OnFailure(() => Mvc.Admin.Email_Preview.Redirect(this))
            .Call();
    }

}
