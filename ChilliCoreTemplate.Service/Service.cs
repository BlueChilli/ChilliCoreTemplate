using AutoMapper;
using ChilliSource.Cloud.Core;
using ChilliSource.Core.Extensions;

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using ChilliCoreTemplate.Models;

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

        protected int? CompanyId { get { return User.UserData() == null ? null : (int?)User.UserData().CompanyId; } }

        internal bool IsAdmin { get { return User.IsInRole(AccountCommon.Administrator); } }

        protected bool IsCompanyAdmin { get { return User.IsInRole(AccountCommon.CompanyAdmin); } }

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

        /// <summary>
        /// Transform T (usually data model) into TX (usually view model) using AutoMapper
        /// </summary>
        /// <typeparam name="TViewModel">Destination Type</typeparam>
        /// <typeparam name="TEntity">Source Type</typeparam>
        /// <param name="entity">Source object</param>
        protected TViewModel GetSingle<TViewModel, TEntity>(TEntity entity)
        {
            TViewModel viewModel = Activator.CreateInstance<TViewModel>();
            Mapper.Map(entity, viewModel);
            return viewModel;
        }

        ///// <summary>
        ///// Using EF, save (insert or update) a view model mapped to a data model to the database
        ///// Used for simple saving when no record ownership checking need to be performed
        ///// </summary>
        ///// <typeparam name="TViewModel">Source Type</typeparam>
        ///// <typeparam name="TEntity">Destination Type</typeparam>
        ///// <param name="set">Destination table</param>
        ///// <param name="viewModel">Data to be saved</param>
        ///// <param name="keyValues">Values of PK to find item to be saved (partial update or full update depending on view model mapping)</param>
        ///// <returns>Returns view model mapped from saved data model (identity column inserted id's filled in)</returns>
        //protected TViewModel Save<TViewModel, TEntity>(TViewModel viewModel, params object[] keyValues) where TEntity : class, new()
        //{
        //    return Context.Save<TViewModel, TEntity>(viewModel, keyValues);
        //}

        ///// <summary>
        ///// Using EF, save (insert or update) a view model mapped to a data model to the database
        ///// Used for simple saving when no record ownership checking need to be performed
        ///// </summary>
        ///// <typeparam name="TViewModel">Source Type</typeparam>
        ///// <typeparam name="TEntity">Destination Type</typeparam>
        ///// <param name="set">Destination table</param>
        ///// <param name="viewModel">Data to be saved</param>
        ///// <param name="keyValues">Values of PK to find item to be saved (partial update or full update depending on view model mapping)</param>
        ///// <returns>Returns view model mapped from saved data model (identity column inserted id's filled in)</returns>
        //protected Task<TViewModel> SaveAsync<TViewModel, TEntity>(DbSet<TEntity> set, TViewModel viewModel, params object[] keyValues) where TEntity : class, new()
        //{
        //    return Context.SaveAsync<TViewModel, TEntity>(viewModel, keyValues);            
        //}

        ///// <summary>
        ///// Using EF, save (insert or update) a view model mapped to a loaded data model to the database. 
        ///// </summary>
        ///// <typeparam name="TViewModel">Source type</typeparam>
        ///// <typeparam name="TDbSet">Destination table type</typeparam>
        ///// <typeparam name="TEntity">Destination type</typeparam>
        ///// <param name="set">Destination table</param>
        ///// <param name="viewModel">Data to be saved</param>
        ///// <param name="entity">Data to be merge into, which is then saved</param>
        ///// <returns></returns>
        //protected TViewModel Save<TViewModel, TDbSet, TEntity>(DbSet<TDbSet> set, TViewModel viewModel, TEntity entity)
        //    where TDbSet : class
        //    where TEntity : class, TDbSet, new()
        //{
        //    return Context.Save<TViewModel, TDbSet, TEntity>(viewModel, entity);            
        //}

        ///// <summary>
        ///// Using EF, save (insert or update) a view model mapped to a loaded data model to the database. 
        ///// </summary>
        ///// <typeparam name="TViewModel">Source type</typeparam>
        ///// <typeparam name="TDbSet">Destination table type</typeparam>
        ///// <typeparam name="TEntity">Destination type</typeparam>
        ///// <param name="set">Destination table</param>
        ///// <param name="viewModel">Data to be saved</param>
        ///// <param name="entity">Data to be merge into, which is then saved</param>
        ///// <returns></returns>
        //protected Task<TViewModel> SaveAsync<TViewModel, TDbSet, TEntity>(DbSet<TDbSet> set, TViewModel viewModel, TEntity entity)
        //    where TDbSet : class
        //    where TEntity : class, TDbSet, new()
        //{
        //    return Context.SaveAsync<TViewModel, TDbSet, TEntity>(viewModel, entity);
        //}

        /// <summary>
        /// Transform a list of T (usually data model) into a list of TX (usually view model) using AutoMapper
        /// </summary>
        /// <typeparam name="TViewModel">Destination Type</typeparam>
        /// <typeparam name="TEntity">Source Type</typeparam>
        /// <param name="entity">Source list</param>
        /// <param name="readOnly">Specifies whether the result will be used for read-only operations.If true, entities will not be added to the current Data Context.</param>
        protected List<TViewModel> GetList<TViewModel, TEntity>(IQueryable<TEntity> query, bool readOnly = false)
            where TEntity : class
        {
            return query.GetList<TEntity, TViewModel>(readOnly);
        }

        /// <summary>
        /// Transform a list of T (usually data model) into a list of TX (usually view model) using AutoMapper
        /// </summary>
        /// <typeparam name="TViewModel">Destination Type</typeparam>
        /// <typeparam name="TEntity">Source Type</typeparam>
        /// <param name="entity">Source list</param>
        /// <param name="readOnly">Specifies whether the result will be used for read-only operations.If true, entities will not be added to the current Data Context.</param>
        protected async Task<List<TViewModel>> GetListAsync<TViewModel, TEntity>(IQueryable<TEntity> query, bool readOnly = false)
            where TEntity : class
        {
            return await query.GetListAsync<TEntity, TViewModel>(readOnly);
        }

        /// <summary>
        /// Transform a list of T (usually data model) into a paged list of TX (usually view model) using AutoMapper
        /// </summary>
        /// <typeparam name="TViewModel">Destination Type</typeparam>
        /// <typeparam name="TEntity">Source Type</typeparam>
        /// <param name="set">Source list</param>
        /// <param name="page">Page to return</param>
        /// <param name="pageSize">Size of each page</param>
        /// <param name="sortBy">Not used</param>
        /// <param name="previousPageIfEmpty">If page is out of bounds, return last page</param>
        /// <param name="readOnly">Specifies whether data entities will be used for read-only operations. If true, entities will not be added to the current Data Context.</param>
        protected PagedList<TViewModel> GetPagedList<TViewModel, TEntity>(IQueryable<TEntity> query, int page = 1, int pageSize = 10, string sortBy = null, bool previousPageIfEmpty = false, bool readOnly = false)
            where TEntity : class
        {
            return query.GetPagedList<TEntity, TViewModel>(page, pageSize, previousPageIfEmpty, readOnly);
        }

        /// <summary>
        /// Transform a list of T (usually data model) into a paged list of TX (usually view model) using AutoMapper
        /// </summary>
        /// <typeparam name="TViewModel">Destination Type</typeparam>
        /// <typeparam name="TEntity">Source Type</typeparam>
        /// <param name="set">Source list</param>
        /// <param name="page">Page to return</param>
        /// <param name="pageSize">Size of each page</param>
        /// <param name="sortBy">Not used</param>
        /// <param name="previousPageIfEmpty">If page is out of bounds, return last page</param>
        /// <param name="readOnly">Specifies whether data entities will be used for read-only operations. If true, entities will not be added to the current Data Context.</param>
        protected async Task<PagedList<TViewModel>> GetPagedListAsync<TViewModel, TEntity>(IQueryable<TEntity> query, int page = 1, int pageSize = 10, string sortBy = null, bool previousPageIfEmpty = false, bool readOnly = false)
            where TEntity : class
        {
            return await query.GetPagedListAsync<TEntity, TViewModel>(page, pageSize, previousPageIfEmpty, readOnly);
        }

        /// <summary>
        /// Pagination on a set of elements.
        /// </summary>
        /// <typeparam name="T">Type of element</typeparam>
        /// <param name="set">Source list</param>
        /// <param name="page">Page to return</param>
        /// <param name="pageSize">Size of each page</param>
        /// <param name="sortBy">Not used</param>
        /// <param name="previousPageIfEmpty">If page is out of bounds, return last page</param>
        /// <param name="readOnly">Specifies whether the result will be used for read-only operations.If true, entities will not be added to the current Data Context.</param>
        protected PagedList<T> GetPagedList<T>(IQueryable<T> query, int page = 1, int pageSize = 10, string sortBy = null, bool previousPageIfEmpty = false, bool readOnly = false)
            where T : class
        {
            return query.GetPagedList(page, pageSize, previousPageIfEmpty, readOnly);
        }

        /// <summary>
        /// Pagination on a set of elements.
        /// </summary>
        /// <typeparam name="T">Type of element</typeparam>
        /// <param name="set">Source list</param>
        /// <param name="page">Page to return</param>
        /// <param name="pageSize">Size of each page</param>
        /// <param name="sortBy">Not used</param>
        /// <param name="previousPageIfEmpty">If page is out of bounds, return last page</param>
        /// <param name="readOnly">Specifies whether the result will be used for read-only operations.If true, entities will not be added to the current Data Context.</param>
        protected Task<PagedList<T>> GetPagedListAsync<T>(IQueryable<T> query, int page = 1, int pageSize = 10, string sortBy = null, bool previousPageIfEmpty = false, bool readOnly = false)
            where T : class
        {
            return query.GetPagedListAsync(page, pageSize, previousPageIfEmpty, readOnly);
        }
    }
}
