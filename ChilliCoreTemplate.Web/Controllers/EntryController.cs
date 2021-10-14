using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using ChilliSource.Cloud.Web.MVC;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliCoreTemplate.Models;
using Microsoft.AspNetCore.Authorization;

namespace ChilliCoreTemplate.Web.Controllers
{
    [CustomAuthorize]
    public class EntryController : Controller
    {
        AccountService _accountSvc;

        public EntryController(AccountService accountSvc)
        {
            _accountSvc = accountSvc;
        }

        public virtual ActionResult Index()
        {
            if (User.IsInRole(AccountCommon.Administrator))
            {
                return Mvc.Admin.Default.Redirect(this);
            }

            if (User.IsInRole(AccountCommon.CompanyAdmin))
            {
                return Mvc.Company.Default.Redirect(this);
            }

            var stepResponse = _accountSvc.GetOnboardingStep();
            if (!stepResponse.Success)
                return Mvc.Root.EmailAccount_Login.Redirect(this);

            //switch (stepResponse.Result)
            //{
            //    case OnboardingStep.SetupCompany:
            //        return Mvc.Root.Company_Setup.Redirect(this);
            //}

            if (User.IsInRole(AccountCommon.User) || User.IsInRole(AccountCommon.CompanyUser))
            {
                return new RedirectResult("~/index.html");
            }

            throw new ApplicationException("No dashboard specified for this user type.");
        }

        public virtual ActionResult ImpersonateRedirect()
        {
            if (User.IsInRole(AccountCommon.Administrator))
            {
                return Mvc.Admin.User_Users.Redirect(this);
            }

            if (User.IsInRole(AccountCommon.CompanyAdmin))
            {
                return Mvc.Company.Default.Redirect(this);
            }

            if (User.IsInRole(AccountCommon.User) || User.IsInRole(AccountCommon.CompanyUser))
            {
                return new RedirectResult("~/user/impersonate");
            }

            return Index();
        }
    }
}
