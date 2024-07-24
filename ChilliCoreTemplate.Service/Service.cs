using ChilliCoreTemplate.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Principal;

namespace ChilliCoreTemplate.Service
{
    /// <summary>
    /// Base interface for Service classes to inherit
    /// </summary>
    public interface IService
    {
    }

    /// <summary>
    /// Base class for Services to inherit where they need to access DbContext.
    /// Consider using instead the AccountBaseService from the Email Account Package.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public abstract class Service<TContext> : IService where TContext : DbContext
    {
        /// <summary>
        /// Gets a DbContext readily available for use. 
        /// Can also be used to set a DbContext (Not recommended).
        /// </summary>
        internal TContext Context { get; private set; }
        
        public IPrincipal User { get; private set; }

        protected int? UserId { get { return User.UserData() == null ? null : (int?)User.UserData().UserId; } }

        internal int? CompanyId { get { return User.UserData() == null ? null : (int?)User.UserData().CompanyId; } }

        internal bool IsAdmin { get { return User != null && User.IsInRole(AccountCommon.Administrator); } }

        protected bool IsCompanyAdmin { get { return User.IsInRole(AccountCommon.CompanyAdmin); } }

        internal bool IsSystem { get { return User.IsInRole(AccountCommon.System); } }

        internal virtual void SetUser(IPrincipal user)
        {
            User = user;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        protected Service(IPrincipal user, TContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context cannot be null.");

            this.Context = context;
            this.User = user;
        }
    }
}
