using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Service.EmailAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ChilliCoreTemplate.Web.Controllers
{
    public class EntryController : Controller
    {
        private readonly ProjectSettings _settings;

        public EntryController(ProjectSettings settings)
        {
            _settings = settings;
        }

        [AllowAnonymous]
        public virtual ActionResult Index()
        {
            return this.RedirectToRoot(_settings);
        }

        [CustomAuthorize]
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
