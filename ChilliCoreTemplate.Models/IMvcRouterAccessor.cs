using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilliCoreTemplate.Models
{
    public interface IMvcRouterAccessor
    {
        IRouter Router { get; }
    }
}
