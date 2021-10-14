using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ChilliSource.Cloud.Web.MVC;
using ChilliSource.Core.Extensions;
using ChilliSource.Cloud.Web;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Security.Principal;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using ChilliCoreTemplate.Web.Api;
using System.Net;

namespace ChilliCoreTemplate.Web
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        public CustomAuthorizeAttribute() { }

        public string[] MultipleRoles
        {
            get
            {
                if (String.IsNullOrEmpty(this.Roles))
                    return new string[0];

                return this.Roles.Split(',');
            }
            set
            {
                if (value != null && value.Length > 0)
                {
                    this.Roles = String.Join(',', value);
                }
                else
                {
                    this.Roles = null;
                }
            }
        }    
    }
}
