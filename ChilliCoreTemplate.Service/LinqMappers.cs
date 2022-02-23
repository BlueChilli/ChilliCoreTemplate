using ChilliSource.Cloud.Core.LinqMapper;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Admin;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Models.EmailAccount;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using ChilliSource.Core.Extensions;
using ChilliCoreTemplate.Service.Admin;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliCoreTemplate.Service.Api;

namespace ChilliCoreTemplate.Service
{
    public static class LinqMappers
    {
        public static void Configure(IServiceProvider serviceProvider)
        {
            var _storagePath = serviceProvider.GetRequiredService<FileStoragePath>();

            LinqMapper.AllowNullPropertyProjection(p => !IsComplexType(p.PropertyType));

            ApiServices.Company_LinqMapper(_storagePath);
            ConfigureViewModels();
            AdminService.Sms_LinqMapper();
            AccountService.PushNotification_LinqMapper();

            //Basic mapping of class
            //LinqMapper.CreateMap<Example, ExampleApiModel>();

            //Basic mapping plus custom mapping
            //LinqMapper.CreateMap<Example, ExampleApiModel>(_DataMap());

            //Basic mapping plus custom mapping with context (dynamic data like UserId)
            //LinqMapper.CreateMap<Example, ExampleApiModel>()
            //    .CreateRuntimeMap<AccountContext>((AccountContext ctx) => _DataMap(ctx));

            //Inline mapper and only return first page in a mapper
            //LinqMapper.CreateMap<Company, CompanyApiModel>(c => new CompanyApiModel()
            //{
            //    BannerImage = c.BannerImagePath,
            //    Projects = c.Projects.Where(p => p.IsActive).OrderBy(p => p.Name).Take(ApiPaging.DefaultPageSize).Select(p => p.InvokeMap<Project, ProjectApiModel>()).ToList()
            //});

            //Aftermap a non linq function (resolve filestorage path)
            //Materializer.RegisterAfterMap<CompanyApiModel>(c =>
            //{
            //    c.BannerImage = FileStoragePath.GetImagePath(c.BannerImage);
            //});

            LinqMapper.CreateMap<UserRole, UserRoleApiModel>();
            LinqMapper.CreateMap<UserRole, LoginRoleModel>();

            LinqMapper.CreateMap<User, UserAccountApiModel>(x => new UserAccountApiModel
            {
                UserRoles = x.UserRoles.Where(r => r.CompanyId == null || !r.Company.IsDeleted).Select(r => r.InvokeMap<UserRole, UserRoleApiModel>()).ToList()
            })
            .IgnoreMembers(a => a.Roles);

            Materializer.RegisterAfterMap<UserAccountApiModel>((a) =>
            {
                a.Roles = a.UserRoles.Select(r => r.Role.ToString()).ToList();
                if (!String.IsNullOrEmpty(a.ProfilePhotoPath))
                {
                    a.ProfilePhotoPath = _storagePath.GetImagePath(a.ProfilePhotoPath, fullPath: true);
                }
            });
        }

        private static void ConfigureViewModels()
        {
            LinqMapper.CreateMap<ErrorLog, ErrorLogSummaryModel>(x => new ErrorLogSummaryModel
            {
                Date = x.TimeStamp,
                UserEmail = x.User == null ? null : x.User.Email,
                Message = x.ExceptionMessage == null ? x.Message.Substring(0, 200) : x.ExceptionMessage.Substring(0, 200)
            });

            LinqMapper.CreateMap<UserRole, UserRoleModel>();
            LinqMapper.CreateMap<User, AccountViewModel>();
            LinqMapper.CreateMap<User, UserSummaryViewModel>(x => new UserSummaryViewModel
            {
                Status = x.Status.ToString(),
                LastLoginOn = x.LastLoginDate == null ? "" : x.LastLoginDate.Value.ToIsoDate()
            });
            Materializer.RegisterAfterMap<UserSummaryViewModel>((x) =>
            {
                var role = x.UserRoles.OrderByDescending(x => x.CompanyId).FirstOrDefault();
                if (role != null)
                {
                    x.Role = role.Role.GetDescription();
                    x.Company = role.CompanyId.HasValue ? new CompanySummaryViewModel { Id = role.CompanyId.Value, Name = role.CompanyName } : null;
                }
            });

            LinqMapper.CreateMap<UserActivity, UserActivityViewModel>();
            LinqMapper.CreateMap<User, LocationUserDetails>(u => new LocationUserDetails
            {
            });

            LinqMapper.CreateMap<Company, EditCompanyViewModel>();
            LinqMapper.CreateMap<Company, CompanyViewModel>(c => new CompanyViewModel
            {
                HasAdmins = c.UserRoles.Any(r => r.Role == Role.CompanyAdmin && r.User.Status != UserStatus.Deleted)
            });
            LinqMapper.CreateMap<Company, CompanyDetailViewModel>(c => new CompanyDetailViewModel()
            {
            }).IncludeBase<Company, CompanyViewModel>();
            LinqMapper.CreateMap<UserRole, CompanyUserViewModel>(u => new CompanyUserViewModel
            {
                Email = u.User.Email,
                Name = u.User.FullName,
                Status = u.User.Status.ToString(),
            });

            LinqMapper.CreateMap<LocationUser, LocationUserViewModel>(u => new LocationUserViewModel
            {
                Id = u.LocationId,
                UserId = u.UserId,
                Email = u.User.Email,
                Name = u.User.FullName,
                Status = u.User.Status.ToString(),
                CreatedOn = u.CreatedOn,
            });

        }

        private static bool IsComplexType(Type propertyType)
        {
            return propertyType.GetCustomAttributes(typeof(ComplexTypeAttribute), false).Length > 0;
        }

        //private static Expression<Func<Example, ExampleApiModel>> _DataMap()
        //{
        //    return (Example e) => new ExampleApiModel()
        //    {
        //        Amount = e.Amount + 100
        //    };
        //}

        //private static Expression<Func<Example, ExampleApiModel>> _DataMap(AccountContext ctx)
        //{
        //    return (Example e) => new ExampleApiModel()
        //    {
        //        Amount = e.Amount + 100,
        //        Birthday = ctx.IsAdmin ? DateTime.UtcNow : e.Birthday
        //    };
        //}

    }

    internal class AccountContext
    {
        public bool IsAdmin { get; set; }

        public int? UserId { get; set; }

        public int? CompanyId { get; set; }
    }

}
