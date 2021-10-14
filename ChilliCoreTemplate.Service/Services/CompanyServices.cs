using AutoMapper;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.LinqMapper;
using ChilliSource.Cloud.Web;
using ChilliSource.Cloud.Web.MVC;
using Microsoft.EntityFrameworkCore;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service.EmailAccount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using DataTables.AspNet.Core;
using ChilliCoreTemplate.Models.Api;

namespace ChilliCoreTemplate.Service
{
    public partial class Services
    {
        private IQueryable<Company> Company_Authorised()
        {
            var query = Context.Companies;

            if (IsAdmin) return query;

            return query.Where(x => !x.IsDeleted);
        }

        public ServiceResult<CompanyDetailViewModel> Company_Get(int? id = null)
        {
            if (id == null) id = CompanyId;

            var model = Company_Authorised()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Materialize<Company, CompanyDetailViewModel>()
                .FirstOrDefault();

            if (model == null) return ServiceResult<CompanyDetailViewModel>.AsError($"Company not found for {id}");

            //if (loadStripeDetails)
            //{
            //    if (company.HasBillingPerSite)
            //    {
            //        if (!String.IsNullOrEmpty(company.SubscriptionJson))
            //            model.StripeDetail = new StripeDetail { Plan = company.SubscriptionJson.FromJson<List<string>>().ToDelimitedString(", ") };
            //    }
            //    else
            //    {
            //        var stripeDetailRequest = Stripe_Get(company.StripeId);
            //        if (!stripeDetailRequest.Success) return ServiceResult<CompanyViewModel>.CopyFrom(stripeDetailRequest);
            //        model.StripeDetail = stripeDetailRequest.Result;
            //    }
            //}

            return ServiceResult<CompanyDetailViewModel>.AsSuccess(model);
        }

        public ServiceResult<CompanyEditModel> Company_Edit(CompanyEditModel model)
        {
            var company = Company_Authorised()
                .Where(x => x.Id == model.Id)
                .FirstOrDefault();

            if (company == null) company = Company.CreateNew(model.Name);

            if (company.Id != model.Id)
            {
                return ServiceResult<CompanyEditModel>.AsError("Company not found");
            }

            var isNew = company.Id == 0;
            Mapper.Map(model, company);

            if (model.LogoFile != null)
                company.LogoPath = this._fileStorage.Save(new StorageCommand() { Folder = "Company" }.SetHttpPostedFileSource(model.LogoFile));

            if (isNew) Context.Companies.Add(company);
            Context.SaveChanges();
            model.Id = company.Id;

            if (String.IsNullOrEmpty(company.StripeId) && model.CreateStripeAccount)
            {
                var stripeResult = Company_AddStripe(company);
                if (!stripeResult.Success) return ServiceResult<CompanyEditModel>.CopyFrom(stripeResult);
            }

            AccountService.Activity_Add(Context, new UserActivity { UserId = UserId.Value, ActivityType = model.Id == 0 ? ActivityType.Create : ActivityType.Update, EntityId = company.Id, EntityType = EntityType.Company });

            return ServiceResult<CompanyEditModel>.AsSuccess(model);
        }

        private ServiceResult<Stripe.Customer> Company_AddStripe(Company company, string token = null)
        {
            var adminEmail = Context.UserRoles.Where(x => x.CompanyId == company.Id && x.Role == Role.CompanyAdmin && x.User.Status != UserStatus.Deleted).Select(x => x.User.Email).FirstOrDefault();
            var customerRequest = _stripe.Customer_Create(new Stripe.CustomerCreateOptions
            {
                Email = adminEmail,
                Description = company.Name,
                Source = token,
                Metadata = new Dictionary<string, string> { { "CompanyId", company.Id.ToString() } }
            });

            if (!customerRequest.Success) return customerRequest;

            company.StripeId = customerRequest.Result.Id;
            Context.SaveChanges();

            return customerRequest;
        }

        public ServiceResult<CompanyEditModel> Company_GetForEdit(int? id = null)
        {
            if (id == null) id = CompanyId;
            var company = Company_Authorised()
                .FirstOrDefault(x => x.Id == id);

            if (company == null && id.GetValueOrDefault(0) != 0) return ServiceResult<CompanyEditModel>.AsError("Company not found");

            var model = Mapper.Map<CompanyEditModel>(company) ?? new CompanyEditModel { ApiKey = Guid.NewGuid(), Timezone = "Australia/Sydney" };
            model.TimezoneList = CommonLibrary.TimeZones().ToSelectList(v => v.ZoneId, t => $"{t.CountryName} {(String.IsNullOrEmpty(t.Comment) ? "" : " - " + t.Comment)}");

            return ServiceResult<CompanyEditModel>.AsSuccess(model);
        }

        public ServiceResult<CompanyListModel> Company_List()
        {
            var model = new CompanyListModel
            {
                Companies = Company_Authorised()
                    .AsNoTracking()
                    .OrderBy(x => x.Name)
                    .Materialize<Company, CompanyViewModel>()
                    .ToList()
            };

            return ServiceResult<CompanyListModel>.AsSuccess(model);
        }

        public ApiPagedList<CompanyViewModel> Company_List(string searchTerm, ApiPaging paging, int? id)
        {
            var query = this.Company_Authorised();

            if (!String.IsNullOrEmpty(searchTerm))
            {
                if (int.TryParse(searchTerm, out var projectId)) query = query.Where(x => x.Id == projectId);
                else query = query.Where(x => x.Name.Contains(searchTerm));
            }
            else if (id.HasValue) query = query.Where(x => x.Id == id.Value);

            return query.Materialize<Company, CompanyViewModel>()
                .Query(q => q.OrderBy(x => x.Name))
                .ToPagedList(paging);
        }

        public ServiceResult<CompanyViewModel> Company_Delete(int id)
        {
            var company = Company_Authorised()
                .Include(x => x.UserRoles)
                .FirstOrDefault(x => x.Id == id);

            if (company != null)
            {
                if (company.IsDeleted || company.UserRoles.Count > 0)
                {
                    company.IsDeleted = !company.IsDeleted;
                }
                else
                {
                    Context.Companies.Remove(company);
                }
                Context.SaveChanges();

                return ServiceResult<CompanyViewModel>.AsSuccess(Mapper.Map<CompanyViewModel>(company));
            }

            return ServiceResult<CompanyViewModel>.AsError("Company not found");
        }

        public ServiceResult<UserDataPrincipal> Company_Impersonate(int companyId, Action<UserDataPrincipal> loginAction)
        {
            var adminAccount = Context.UserRoles.FirstOrDefault(a => a.CompanyId == companyId && a.User.Status != UserStatus.Deleted && a.Role.HasFlag(Role.CompanyAdmin));

            if (adminAccount == null) return ServiceResult<UserDataPrincipal>.AsError("Company does not have administrator account that can be impersonated");

            return _accountService.ImpersonateAccount(adminAccount.UserId, loginAction);

        }

        public ServiceResult<CompanyDetailViewModel> Company_Admin_Add(int id, InviteEditModel model)
        {
            var company = Company_Authorised()
                .FirstOrDefault(x => x.Id == id);
            Context.Entry(company)
                .Collection(x => x.UserRoles)
                .Query()
                .Where(u => u.User.Email == model.Email)
                .ToList();

            if (company == null) return ServiceResult<CompanyDetailViewModel>.AsError("Company not found");
            if (company.UserRoles.Any()) return ServiceResult<CompanyDetailViewModel>.AsError($"Company already has the user {model.Email}");

            var user = _accountService.GetAccountByEmail(model.Email);
            var userId = user?.Id;

            if (user == null)
            {   //Invite code will add user to company
                var newUserRequest = _accountService.Invite(model, true);
                if (!newUserRequest.Success) return ServiceResult<CompanyDetailViewModel>.CopyFrom(newUserRequest);
                userId = newUserRequest.Result.Id;
            }
            Context.SaveChanges();

            return ServiceResult<CompanyDetailViewModel>.AsSuccess(Company_Get(id).Result);
        }

        public PagedList<CompanyUserViewModel> Company_Admin_List(IDataTablesRequest model, int id, Role role)
        {
            var query = Context.UserRoles
                .Where(x => x.CompanyId == id && (x.Role & role) > 0);

            if (!String.IsNullOrEmpty(model.Search.Value))
            {
                query = query.Where(x => x.User.Email.StartsWith(model.Search.Value));
            }

            var isNew = DateTime.UtcNow.AddMinutes(-5);
            var queryOrdered = query.OrderBy(x => x.CreatedAt > isNew ? 0 : 1);

            foreach (var column in model.Columns.Where(c => c.IsSortable))
            {
                switch (column.Field)
                {
                    case "name":
                        if (column.IsSortable)
                        {
                            if (column.Sort.Direction == SortDirection.Ascending) queryOrdered = queryOrdered.ThenBy(x => x.User.FullName);
                            else queryOrdered = queryOrdered.ThenByDescending(x => x.User.FullName);
                        }
                        break;
                }
            }

            var users = queryOrdered
                .Materialize<UserRole, CompanyUserViewModel>()
                .ToPagedList(model.Start / model.Length + 1, model.Length);

            return users;
        }

        public int Company_Admin_Count(int id, Role role)
        {
            var query = Context.UserRoles
                .Where(x => x.CompanyId == id && (x.Role & role) > 0);

            return query.Count();
        }

        public ServiceResult<CompanyUserViewModel> Company_Admin_Get(int id, int userId)
        {
            var user = Context.UserRoles
                .Where(x => x.CompanyId == id && x.UserId == userId)
                .Materialize<UserRole, CompanyUserViewModel>()
                .FirstOrDefault();

            if (user == null) return ServiceResult<CompanyUserViewModel>.AsError("User not found");

            return ServiceResult<CompanyUserViewModel>.AsSuccess(user);
        }

        public ServiceResult Company_Admin_Delete(int id, int userId)
        {
            var user = Context.UserRoles
                .FirstOrDefault(x => x.CompanyId == id && x.UserId == userId);

            if (user == null) return ServiceResult.AsError("User not found");

            Context.UserRoles.Remove(user);
            Context.SaveChanges();

            return ServiceResult.AsSuccess();
        }

        public AccountViewModel Company_Admin_Details(int id, string email, Role role)
        {
            var emailHash = CommonLibrary.CalculateHash(email);
            var user = _accountService.VisibleUsers()
                .Where(x => x.EmailHash == emailHash && x.Email == email && x.UserRoles.Any(r => r.CompanyId == id && (r.Role & role) > 0))
                .Materialize<User, AccountViewModel>()
                .FirstOrDefault();

            return user ?? new AccountViewModel();
        }
    }    
}
