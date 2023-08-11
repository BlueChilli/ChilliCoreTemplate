using AutoMapper;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.LinqMapper;
using ChilliSource.Cloud.Web;
using ChilliSource.Cloud.Web.MVC;
using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace ChilliCoreTemplate.Service.EmailAccount
{
    public class CompanyService : BaseService
    {
        private readonly AccountService _accountService;
        private readonly StripeService _stripe;

        public CompanyService(IPrincipal user, DataContext context, ProjectSettings config, IFileStorage fileStorage, IWebHostEnvironment environment,
            AccountService accountService, StripeService stripeService) : base(user, context, config, fileStorage, environment)
        {
            _accountService = accountService;
            _stripe = stripeService;
        }


        internal IQueryable<Company> Authorised()
        {
            var query = Context.Companies;

            if (IsAdmin) return query;

            return query.Where(x => !x.IsDeleted);
        }

        internal IQueryable<Company> Authorised(int id)
        {
            var query = Context.Companies.Where(x => x.Id == id);

            if (IsAdmin) return query;

            return query.Where(x => !x.IsDeleted);
        }

        public ServiceResult<CompanyDetailViewModel> Get(int? id = null)
        {
            if (id == null) id = CompanyId;

            var model = Authorised()
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

        public ServiceResult<CompanyEditModel> Edit(CompanyEditModel model)
        {
            var record = Authorised()
                .Where(x => x.Id == model.Id)
                .FirstOrDefault();

            if (record == null) record = Company.CreateNew(model.Name);

            if (record.Id != model.Id)
            {
                return ServiceResult<CompanyEditModel>.AsError("Company not found");
            }

            model.Name = model.Name.Trim();
            var hasDuplicate = Context.Companies.Where(c => c.Name == model.Name && c.Id != model.Id && !c.IsDeleted).Any();
            if (hasDuplicate)
                return ServiceResult<CompanyEditModel>.AsError($"Company '{model.Name}' already exists.");

            var isNew = record.Id == 0;
            Mapper.Map(model, record, opts => opts.Items["IsAdmin"] = IsAdmin);

            if (model.LogoFile != null)
                record.LogoPath = this._fileStorage.Save(new StorageCommand() { Folder = "Company" }.SetHttpPostedFileSource(model.LogoFile));

            if (isNew) Context.Companies.Add(record);
            Context.SaveChanges();
            model.Id = record.Id;

            if (String.IsNullOrEmpty(record.StripeId) && model.CreateStripeAccount)
            {
                var stripeResult = AddStripe(record);
                if (!stripeResult.Success) return ServiceResult<CompanyEditModel>.CopyFrom(stripeResult);
            }

            AccountService.Activity_Add(Context, new UserActivity { UserId = UserId.Value, ActivityType = model.Id == 0 ? ActivityType.Create : ActivityType.Update, EntityId = record.Id, EntityType = EntityType.Company });

            return ServiceResult<CompanyEditModel>.AsSuccess(model);
        }

        public ServiceResult SaveSettings(CompanySettingsModel model)
        {
            var record = Authorised()
                .Where(x => x.Id == CompanyId.Value)
                .FirstOrDefault();

            if (record == null) return ServiceResult.AsError("Company not found");

            model.Name = model.Name.Trim();
            var hasDuplicate = Context.Companies.Where(c => c.Name == model.Name && c.Id != CompanyId.Value && !c.IsDeleted).Any();
            if (hasDuplicate)
                return ServiceResult.AsError($"Company '{model.Name}' already exists.");

            Mapper.Map(model, record, opts => opts.Items["IsAdmin"] = IsAdmin);

            if (model.LogoFile != null)
                record.LogoPath = this._fileStorage.Save(new StorageCommand() { Folder = "Company" }.SetHttpPostedFileSource(model.LogoFile));

            Context.SaveChanges();

            _accountService.Session_Clear(User.Session()?.Id);

            return ServiceResult.AsSuccess();
        }

        private ServiceResult<Stripe.Customer> AddStripe(Company company, string token = null)
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

        public ServiceResult<CompanyEditModel> GetForEdit(int? id = null, string name = null)
        {
            if (id == null) id = User.UserData().IsMasterCompany ? 0 : CompanyId ?? 0;
            var model = Authorised(id.Value)
                .Materialize<Company, CompanyEditModel>()
                .FirstOrDefault();

            if (model == null && id.GetValueOrDefault(0) != 0) return ServiceResult<CompanyEditModel>.AsError("Company not found");

            if (model == null) model = new CompanyEditModel { ApiKey = Guid.NewGuid(), Timezone = "Australia/Sydney", Name = name };

            model.TimezoneList = CommonLibrary.TimeZones().ToSelectList(v => v.ZoneId, t => $"{t.CountryName} {(String.IsNullOrEmpty(t.Comment) ? "" : " - " + t.Comment)}");

            return ServiceResult<CompanyEditModel>.AsSuccess(model);
        }

        public ServiceResult<CompanySettingsModel> GetSettings()
        {
            var model = Authorised(CompanyId.Value)
                .Materialize<Company, CompanySettingsModel>()
                .FirstOrDefault();

            if (model == null) return ServiceResult<CompanySettingsModel>.AsError("Company not found");

            model.TimezoneList = CommonLibrary.TimeZones().ToSelectList(v => v.ZoneId, t => $"{t.CountryName} {(String.IsNullOrEmpty(t.Comment) ? "" : " - " + t.Comment)}");

            return ServiceResult<CompanySettingsModel>.AsSuccess(model);
        }

        public PagedList<CompanySummaryModel> List(IDataTablesRequest model)
        {
            var filterModel = new CompanyListModel();

            if (!String.IsNullOrEmpty(model.Search.Value))
            {
                filterModel.Search = model.Search.Value;
            }

            foreach (var column in model.Columns)
            {
                if (!String.IsNullOrEmpty(column.Search.Value))
                {
                    switch (String.IsNullOrEmpty(column.Name) ? column.Field : column.Name)
                    {
                        case "status":
                            filterModel.Status = bool.Parse(column.Search.Value);
                            break;
                    }
                }
            }

            var query = ListQuery(filterModel);

            var mapping = new DataTableColumnMapping<Company>();
            mapping.SetDefaultOrder(x => x.Name, ascending: true);
            mapping.Add("Created", x => x.CreatedAt);
            mapping.Add("Name", x => x.Name);
            query = mapping.ApplyOrder(query, model);

            var data = query
                .Materialize<Company, CompanySummaryModel>()
                .ToPagedList(model.Start / model.Length + 1, model.Length);
            return data;
        }

        public int Company_Count()
        {
            var query = ListQuery(new CompanyListModel());

            if (query == null) return 0;

            return query.Count();
        }

        private IQueryable<Company> ListQuery(CompanyListModel model)
        {
            var query = Authorised();

            if (!String.IsNullOrEmpty(model.Search))
            {
                query = query.Where(x => x.Name.StartsWith(model.Search));
            }
            if (model.Status.HasValue)
            {
                if (model.Status.Value) query = query.Where(x => !x.IsDeleted);
                else query = query.Where(x => x.IsDeleted);
            }

            return query;
        }

        public ApiPagedList<DataLinkModel> List(string searchTerm, ApiPaging paging, int? id)
        {
            var query = this.Authorised();

            if (!String.IsNullOrEmpty(searchTerm))
            {
                if (int.TryParse(searchTerm, out var companyId)) query = query.Where(x => x.Id == companyId);
                else query = query.Where(x => x.Name.Contains(searchTerm));
            }
            else if (id.HasValue) query = query.Where(x => x.Id == id.Value);

            return query.Materialize<Company, DataLinkModel>()
                .Query(q => q.OrderBy(x => x.Name))
                .ToPagedList(paging);
        }

        public List<T> List<T>(bool includeDeleted = false) where T : class, new()
        {
            var query = this.Authorised();

            if (!includeDeleted) query = query.Where(x => !x.IsDeleted);

            return query.Materialize<Company, T>().ToList();
        }

        public ServiceResult<CompanyViewModel> Delete(int id)
        {
            var data = Authorised()
                .Include(x => x.UserRoles)
                .FirstOrDefault(x => x.Id == id);

            if (data != null)
            {
                var inUse = data.UserRoles.Count > 0;
                if (data.IsDeleted || inUse)
                {
                    data.IsDeleted = !data.IsDeleted;
                    if (data.IsDeleted)
                    {
                        data.DeletedAt = DateTime.UtcNow;
                        data.DeletedById = UserId;
                    }
                    else
                    {
                        data.UpdatedAt = DateTime.UtcNow;
                        data.DeletedAt = null;
                        data.DeletedById = null;
                    }
                }
                else
                {
                    Context.Companies.Remove(data);
                }
                Context.SaveChanges();

                return ServiceResult<CompanyViewModel>.AsSuccess(Mapper.Map<CompanyViewModel>(data));
            }

            return ServiceResult<CompanyViewModel>.AsError("Company not found");
        }

        public ServiceResult<CompanyViewModel> Purge(int id)
        {
            var company = Authorised()
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.User)
                .FirstOrDefault(x => x.Id == id);

            if (company != null)
            {
                for (var i = 0; i < company.UserRoles.Count; i++)
                {
                    var role = company.UserRoles[i];
                    if (role.Role == Role.User) role.CompanyId = null;
                    else
                    {
                        _accountService.Purge(role.UserId);
                    }
                }

                Context.SaveChanges();

                Context.Companies.Remove(company);
                Context.SaveChanges();

                if (!String.IsNullOrEmpty(company.LogoPath) && _fileStorage.Exists(company.LogoPath)) _fileStorage.Delete(company.LogoPath);

                if (!String.IsNullOrEmpty(company.StripeId)) _stripe.Customer_Delete(company.StripeId);

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
            var company = Authorised()
                .FirstOrDefault(x => x.Id == id);

            Context.Entry(company)
                .Collection(x => x.UserRoles)
                .Query()
                .Where(u => u.User.Email == model.Email)
                .ToList();

            if (company == null) return ServiceResult<CompanyDetailViewModel>.AsError("Company not found");
            if (company.UserRoles.Any()) return ServiceResult<CompanyDetailViewModel>.AsError($"Company already has the user {model.Email}");

            var user = _accountService.GetAccountByEmail(model.Email);
            if (user == null)
            {
                var newUserRequest = _accountService.Invite(model, true);
                if (!newUserRequest.Success) return ServiceResult<CompanyDetailViewModel>.CopyFrom(newUserRequest);
            }
            else
            {
                user.UserRoles.Add(new UserRole { CompanyId = id, Role = Role.CompanyAdmin });
            }
            Context.SaveChanges();

            return ServiceResult<CompanyDetailViewModel>.AsSuccess(Get(id).Result);
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
