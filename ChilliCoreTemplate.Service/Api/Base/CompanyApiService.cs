using AutoMapper;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.LinqMapper;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;

namespace ChilliCoreTemplate.Service.Api
{
    public class CompanyApiService : BaseApiService
    {
        private AccountService _accountService;

        public CompanyApiService(IPrincipal user, DataContext context, ProjectSettings config, IFileStorage fileStorage, IWebHostEnvironment environment, AccountService accountService, IMapper mapper) 
            : base(user, context, config, fileStorage, environment, mapper)
        {
            _accountService = accountService;
        }

        internal static void LinqMapper_Config(FileStoragePath _storagePath)
        {
            LinqMapper.CreateMap<Company, CompanyApiModel>(x => new CompanyApiModel
            {
            });
            Materializer.RegisterAfterMap<CompanyApiModel>((x) =>
            {
                if (!String.IsNullOrEmpty(x.LogoPath))
                {
                    x.LogoPath = _storagePath.GetImagePath(x.LogoPath, fullPath: true);
                }
            });

            LinqMapper.CreateMap<UserRole, CompanyUserApiModel>(x => new CompanyUserApiModel
            {
                Id = x.UserId,
                FirstName = x.User.FirstName,
                LastName = x.User.LastName,
                Email = x.User.Email,
                LastLoginOn = x.User.LastLoginDate,
                Status = x.User.Status
            });

        }

        private IQueryable<Company> Authorised()
        {
            var query = Context.Companies;

            return query.Where(x => x.Id == CompanyId && !x.IsDeleted);
        }

        public ServiceResult<List<CompanyApiModel>> List(CompanyFilterApiModel model)
        {
            var query = Context.Companies.Where(x => !x.IsDeleted);

            if (!String.IsNullOrEmpty(model.Search)) query = query.Where(x => x.Name.Contains(model.Search));
            if (model.LastChangedAt.HasValue) query = query.Where(x => x.UpdatedAt >= model.LastChangedAt.Value);

            var records = query
                .OrderBy(x => x.Name)
                .Materialize<Company, CompanyApiModel>()
                .ToList();

            return ServiceResult<List<CompanyApiModel>>.AsSuccess(records);
        }

        public ServiceResult Create(CompanyEditApiModel model)
        {
            var company = Company.CreateNew(model.Name);

            if (!String.IsNullOrWhiteSpace(model.LogoFileBase64))
            {
                company.LogoPath = _fileStorage.Save(new StorageCommand { Folder = "Company", Extension = Path.GetExtension(model.LogoFileName) }.SetByteArraySource(Convert.FromBase64String(model.LogoFileBase64)));
            }

            Context.Companies.Add(company);
            Context.SaveChanges();

            var user = _accountService.GetAccount(UserId.Value);
            _accountService.AddNewAccountRoles(user, new RoleSelectionViewModel() { Role = Role.CompanyAdmin, CompanyId = company.Id });
            Context.SaveChanges();

            _accountService.Session_Replace(User.Session().Id, user, User.UserData().UserDeviceId);

            return ServiceResult.AsSuccess();
        }

        public ServiceResult Update(CompanyEditApiModel model)
        {
            var company = Context.Companies.Where(x => x.Id == CompanyId).FirstOrDefault();
            if (company == null) return ServiceResult.AsError("Compnay not found");

            if (!String.IsNullOrWhiteSpace(model.LogoFileBase64))
            {
                company.LogoPath = _fileStorage.Save(new StorageCommand { Folder = "Company", Extension = Path.GetExtension(model.LogoFileName) }.SetByteArraySource(Convert.FromBase64String(model.LogoFileBase64)));
            }
            company.Name = model.Name;

            Context.SaveChanges();

            return ServiceResult.AsSuccess();
        }

        public ServiceResult<CompanyApiModel> Get()
        {
            var company = Authorised().Materialize<Company, CompanyApiModel>().FirstOrDefault();
            if (company == null) return ServiceResult<CompanyApiModel>.AsError("Company not found");
            return ServiceResult<CompanyApiModel>.AsSuccess(company);
        }

        #region CompanyAdmin
        private IQueryable<UserRole> Admin_Authorised()
        {
            return Context.UserRoles.Where(x => x.CompanyId == CompanyId && (x.Role == Role.CompanyAdmin || x.Role == Role.CompanyUser));
        }
        private IQueryable<UserRole> Admin_Authorised(int userId)
        {
            return Admin_Authorised().Where(x => x.UserId == userId);
        }

        public List<CompanyUserApiModel> Admin_List()
        {
            var users = Admin_Authorised()
                .Materialize<UserRole, CompanyUserApiModel>()
                .ToList();

            return users;
        }

        public ServiceResult<CompanyUserApiModel> Admin_Get(int id)
        {
            var user = Admin_Authorised(id)
                .Materialize<UserRole, CompanyUserApiModel>()
                .FirstOrDefault();

            if (user == null) return ServiceResult<CompanyUserApiModel>.AsError("User not found");

            return ServiceResult<CompanyUserApiModel>.AsSuccess(user);
        }

        public ServiceResult Admin_Update(CompanyUserEditApiModel model)
        {
            var userRole = Admin_Authorised(model.Id)
                .Include(x => x.User)
                .FirstOrDefault();

            if (userRole == null) return ServiceResult.AsError("User not found in company");

            if (String.IsNullOrEmpty(userRole.User.Email) || !userRole.User.Email.Same(model.Email))
            {
                if (_accountService.Exists(model.Email, model.Id)) return ServiceResult.AsError("Email address already exists");
            }

            userRole.User.FirstName = model.FirstName;
            userRole.User.LastName = model.LastName;
            userRole.User.Email = model.Email;
            Context.SaveChanges();

            if (userRole.Status == RoleStatus.Invited) _accountService.Reinvite(userRole.UserId);

            return ServiceResult.AsSuccess();
        }

        public ServiceResult Admin_Delete(int userId)
        {
            var usersRoles = Context.UserRoles
                .Where(x => x.UserId == userId)
                .ToList();

            var companyRole = usersRoles.Where(x => x.CompanyId == CompanyId).FirstOrDefault();

            if (companyRole != null)
            {
                Context.UserRoles.Remove(companyRole);

                if (!usersRoles.Any(x => x.CompanyId != CompanyId))
                    Context.UserRoles.Add(new UserRole { UserId = userId, Role = Role.User, CreatedAt = DateTime.UtcNow });

                Context.SaveChanges();
            }

            return ServiceResult.AsSuccess();
        }
        #endregion

    }

}
