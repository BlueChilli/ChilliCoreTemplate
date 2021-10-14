using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ChilliCoreTemplate.Models;
using Microsoft.AspNetCore.Mvc;


namespace ChilliCoreTemplate.Web.Controllers
{
    public class PublicController : Controller
    {

        readonly ProjectSettings _settings;

        public PublicController(ProjectSettings settings)
        {
            _settings = settings;
        }

        public virtual ActionResult Index()
        {
            return this.RedirectToRoot(_settings);
        }
    }
}
