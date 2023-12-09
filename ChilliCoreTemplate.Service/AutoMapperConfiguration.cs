using AutoMapper;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service.Api;
using ChilliCoreTemplate.Service.EmailAccount;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChilliCoreTemplate.Service
{
    public class AutoMapperConfiguration : Profile
    {
        public AutoMapperConfiguration() 
        {
            ProjectConfigure();
            Configure();
        }

        private void ProjectConfigure()
        {
            CreateMap<CompanyEditModel, Company>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Condition(src => src.Id == 0))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Guid, opt => opt.Condition(src => src.Id == 0))
                .ForMember(dest => dest.Guid, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.LogoPath, opt => opt.Ignore())
                .ForMember(dest => dest.Notes, opt => opt.Condition((src, dest, m1, m2, opts) => bool.Parse(opts.Items["IsAdmin"].ToString())));
            CreateMap<Company, CompanyViewModel>();

            CreateMap<CompanySettingsModel, Company>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.LogoPath, opt => opt.Ignore());
        }

        public void Configure()
        {
            CreateMap<UserRole, UserRoleModel>();
            CreateMap<UserRole, RoleSelectionViewModel>();
            CreateMap<UserRole, InviteRoleViewModel>();

            CreateMap<User, UserData>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.CurrentRoles, opt => opt.MapFrom(src => src.UserRoles.Where(x => x.CompanyId == null || x.Company == null || !x.Company.IsDeleted)))
                .AfterMap((src, dest, ctx) =>
                {
                    if (!dest.CurrentRoles.Any()) dest.SetCurrentRoles(new List<UserRoleModel> { new UserRoleModel { Role = Role.User } });
                    var company = src.GetFirstCompany();
                    if (company != null)
                    {
                        dest.CompanyId = company.Id;
                        dest.CompanyLogoPath = company.LogoPath;
                        dest.CompanyName = company.Name;
                        dest.Timezone = company.Timezone;
                        //TODO add check that mastercompany flag is turned on
                        if (ctx.Items.ContainsKey("DataContext"))
                        {
                            var context = ctx.Items["DataContext"] as DataContext;
                            var isMasterCompany = context.Companies.Any(x => x.MasterCompanyId == company.Id);
                            if (isMasterCompany) dest.IsMasterCompany = true;
                        }
                    }
                });

            CreateMap<UserData, UserData>();

            CreateMap<User, AccountViewModel>().ReverseMap();

            CreateMap<AccountDetailsEditModel, User>()
                .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.ProfilePhotoPath, opt => opt.Condition(src => src.ProfilePhotoFile != null));

            CreateMap<UserActivity, UserActivityViewModel>()
                .ForMember(dest => dest.Account, opt => opt.MapFrom(src => src.User));
            CreateMap<UserActivityViewModel, UserActivity>();

            CreateMap<RoleSelectionViewModel, RoleSelectionViewModel>();

            CreateMap<AccountViewModel, RegistrationViewModel>();
            CreateMap<RegistrationApiModel, RegistrationViewModel>()
                .ForMember(dest => dest.MixpanelTempId, opt => opt.MapFrom(src => src.AnonymousUserId));

            CreateMap<RegistrationViewModel, UserCreateModel>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsAnonymous ? UserStatus.Anonymous : UserStatus.Registered))
                .AfterMap((src, dest) =>
                    {
                        dest.UserRoles = new List<RoleSelectionViewModel> { new RoleSelectionViewModel { Role = src.Roles, CompanyName = src.CompanyName, CompanyGuid = src.CompanyGuid } };
                    });
            CreateMap<InviteEditModel, UserCreateModel>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => UserStatus.Registered))
                .AfterMap((src, dest) =>
                {
                    dest.UserRoles = new List<RoleSelectionViewModel> { new RoleSelectionViewModel { Role = src.InviteRole.Role.Value, CompanyName = src.InviteRole.CompanyName, CompanyId = src.InviteRole.CompanyId, Status = RoleStatus.Invited } };
                });

            CreateMap<UserCreateModel, User>()
                .ForMember(dest => dest.UserRoles, opt => opt.Ignore());

            CreateMap<InviteEditModel, User>()
                .ForMember(dest => dest.InvitedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => UserStatus.Registered));

            CreateMap<User, InviteEditModel>()
                .ForMember(dest => dest.InviteRole, opt => opt.MapFrom(src => src.GetLatestUserRole()))
                .ForMember(dest => dest.Token, opt => opt.MapFrom(src => src.GetToken(UserTokenType.Invite)));

            CreateMap<InviteEditApiModel, InviteEditModel>()
                .ForMember(dest => dest.InviteRole, opt => opt.MapFrom((src, dest, param, ctx) => new InviteRoleViewModel { Role = src.Role, CompanyId = (int)ctx.Items["CompanyId"] }));

            CreateMap<InviteUploadItemModel, InviteEditModel>();

            CreateMap<Email, EmailSummaryModel>();
            CreateMap<Email, EmailViewModel>();
            CreateMap<Email, EmailUnsubscribeModel>();
        }
    }
}
