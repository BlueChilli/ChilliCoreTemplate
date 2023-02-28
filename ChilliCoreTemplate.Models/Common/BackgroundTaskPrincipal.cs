using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Models
{
    public class BackgroundTaskPrincipal : GenericPrincipal
    {
        public BackgroundTaskPrincipal()
            : base(new GenericIdentity("BackgroundTaskIdentity"), new string[] { AccountCommon.System })
        {
        }
    }
}
