using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web
{
    public interface ITypedRoute
    {

    }

    public class TypedRoute
    {

    }

    public class TypedRoutes
    {
        private TypedRoutes() { }

        public static readonly TypedRoutes Instance = new TypedRoutes();

        public readonly TypedRouteEmailAccount EmailAccount = new TypedRouteEmailAccount();
    }

    public class TypedRouteEmailAccount : ITypedRoute
    {                
        public readonly TypedRoute Login = new TypedRoute();
    }
}
