using AutoMapper;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.LinqMapper;

using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using ChilliSource.Cloud.Web;
using StackExchange.Profiling;

namespace ChilliCoreTemplate.Service.EmailAccount
{
    public class CompanyService : Service<DataContext>
    {
        IFileStorage _fileStorage;
        UserSessionService _sessionService;

        public CompanyService(IPrincipal user, DataContext context, IFileStorage fileStorage, UserSessionService sessionService)
            : base(user, context)
        {
            _fileStorage = fileStorage;
            _sessionService = sessionService;
        }

        internal static Expression<Func<Company, bool>> VisibleCompaniesExp(UserData userData, bool includeDeleted)
        {
            if (userData == null)
                return (Company o) => false;

            if (userData.IsInRole(Role.Administrator))
                return (Company o) => includeDeleted || !o.IsDeleted;

            var CompanyIds = userData.GetCompanyIds();

            return (Company o) => CompanyIds.Contains(o.Id) && (includeDeleted || !o.IsDeleted);
        }

        internal IQueryable<Company> VisibleCompanies(bool includeDeleted = false)
        {
            var userData = User.UserData();

            if (userData == null)
                return Enumerable.Empty<Company>().AsQueryable();

            return Context.Companies.Where(VisibleCompaniesExp(userData, includeDeleted));
        }

        public List<T> GetAll<T>(bool includeDeleted = false) where T : class, ICompanyViewModel, new()
        {
            using (MiniProfiler.Current.Step("CompanyService.GetAll"))
            {
                return this.VisibleCompanies(includeDeleted)
                          .OrderByDescending(o => o.CreatedAt)
                          .Materialize<Company, T>()
                          .ToList();
            }
        }

        public ServiceResult<T> Get<T>(int id, bool includeDeleted = false) where T : class, ICompanyViewModel, new()
        {
            var model = this.VisibleCompanies(includeDeleted)
                            .Where(o => o.Id == id)
                            .Materialize<Company, T>()
                            .FirstOrDefault();

            if (model == null)
                return ServiceResult<T>.AsError("Not found or acess denied");

            return ServiceResult<T>.AsSuccess(model);
        }

        public void TestDBException()
        {            
            var company = Context.Companies.Add(new Company()).Entity;
            var user = Context.Users.Add(new Data.EmailAccount.User()).Entity;
            var role = Context.UserRoles.Add(new UserRole() { User = user, Company = company, Role = Role.User, CreatedAt = DateTime.UtcNow });

            Context.SaveChanges();
        }
    }
}
