using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Mvc;
using ChilliCoreTemplate.Models;

namespace ChilliCoreTemplate.Web.Areas.Admin.Controllers
{

    [Area("Admin")]
    [CustomAuthorize(Roles = AccountCommon.Administrator)]
    [Mfa]
    public class DefaultController : Controller
    {

        public virtual ActionResult Index()
        {
            return Mvc.Admin.Company_List.Redirect(this);
        }
    }
}
