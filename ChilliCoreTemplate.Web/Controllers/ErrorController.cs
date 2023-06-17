using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Mvc;


namespace ChilliCoreTemplate.Web.Controllers
{
    public class ErrorController : Controller
    {
        public virtual ActionResult Index()
        {
            if (Request.IsAjaxRequest())
            {
                return PartialView();
            }

            return View();
        }

        public new ActionResult NotFound()
        {
            if (Request.IsAjaxRequest())
            {
                return PartialView();
            }

            return View();
        }

        public virtual ActionResult TestException()
        {
            ThrowExceptionMethod();
            return this.Ok();
        }

        private void ThrowExceptionMethod()
        {
            throw new ApplicationException("This is a test exception");
        }
    }
}
