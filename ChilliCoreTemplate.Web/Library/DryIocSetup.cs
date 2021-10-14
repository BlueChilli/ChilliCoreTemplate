using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web
{
    public static class DryIocSetup
    {
        public static IContainer MvcContainer { get; private set; }

        public static IServiceProvider Initialise(IServiceCollection services)
        {
            MvcContainer = new DryIoc.Container().WithDependencyInjectionAdapter(services);

            return MvcContainer.BuildServiceProvider();
        }
    }
}
