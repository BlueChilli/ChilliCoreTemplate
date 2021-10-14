using ChilliCoreTemplate.Models;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web
{
    public class MvcRouterAccessor: IMvcRouterAccessor
    {
        public IRouter Router { get; set; }
    }
}
