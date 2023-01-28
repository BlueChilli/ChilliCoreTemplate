using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Mvc;
using ChilliCoreTemplate.Models;

namespace ChilliCoreTemplate.Web.Areas.Company.Controllers
{

    [Area("Company")]
    [CustomAuthorize(MultipleRoles = new string[] { AccountCommon.CompanyAdmin, AccountCommon.CompanyUser })]
    public class DefaultController : Controller
    {

        public virtual ActionResult Index()
        {
            return Mvc.Company.User_List.Redirect(this);
        }
    }
}
