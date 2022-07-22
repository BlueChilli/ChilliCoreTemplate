using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Service.EmailAccount;
using Microsoft.AspNetCore.Mvc;
using System;

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

            if (User.IsInRole(AccountCommon.CompanyAdmin) || User.IsInRole(AccountCommon.CompanyUser))
            {
                return Mvc.Company.Default.Redirect(this);
            }

            if (User.IsInRole(AccountCommon.User))
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

            if (User.IsInRole(AccountCommon.CompanyAdmin) || User.IsInRole(AccountCommon.CompanyUser))
            {
                return Mvc.Company.Default.Redirect(this);
            }

            if (User.IsInRole(AccountCommon.User))
            {
                return new RedirectResult("~/user/impersonate");
            }

            return Index();
        }
    }
}
