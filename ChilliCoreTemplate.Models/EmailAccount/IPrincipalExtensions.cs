using ChilliSource.Cloud.Core; using ChilliSource.Cloud.Web.MVC;
using ChilliCoreTemplate.Models.EmailAccount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Models
{
    public static partial class IPrincipalExtensions
    {
        public static bool IsAuthenticated(this IPrincipal principal)
        {
            return principal?.Identity?.IsAuthenticated == true;
        }

        public static UserData UserData(this IPrincipal principal)
        {
            return principal.Session()?.UserData;
        }

        public static UserDataPrincipal Session(this IPrincipal principal)
        {
            if (principal is UserDataPrincipal) return principal as UserDataPrincipal;

            return null;
        }
    }
}
