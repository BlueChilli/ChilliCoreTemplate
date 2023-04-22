using AutoMapper;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service.Api;
using ChilliCoreTemplate.Service.EmailAccount;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChilliCoreTemplate.Service
{
    public class AutoMapperConfiguration
    {
        private static void ProjectConfigure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<CompanyEditModel, Company>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Condition(src => src.Id == 0))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Guid, opt => opt.Condition(src => src.Id == 0))
                .ForMember(dest => dest.Guid, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.LogoPath, opt => opt.Ignore())
                .ForMember(dest => dest.Notes, opt => opt.Condition((src, dest, m1, m2, opts) => bool.Parse(opts.Items["IsAdmin"].ToString())));
            cfg.CreateMap<Company, CompanyViewModel>();
        }

        public static void Configure()
        {
            Mapper.Initialize(cfg =>
            {
                BulkImportService.AutoMapper(cfg);

                cfg.CreateMap<UserRole, UserRoleModel>();
                cfg.CreateMap<UserRole, RoleSelectionViewModel>();
                cfg.CreateMap<UserRole, InviteRoleViewModel>();

                cfg.CreateMap<User, UserData>()
                    .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                    .AfterMap((src, dest, ctx) =>
                    {
                        var roles = Mapper.Map<List<UserRole>, List<UserRoleModel>>(src.UserRoles.Where(x => x.CompanyId == null || x.Company == null || !x.Company.IsDeleted).ToList());
                        if (!roles.Any()) roles.Add(new UserRoleModel { Role = Role.User });
                        dest.SetCurrentRoles(roles);
                        var company = src.GetFirstCompany();
                        if (company != null)
                        {
                            dest.CompanyId = company.Id;
                            dest.CompanyLogoPath = company.LogoPath;
                            dest.CompanyName = company.Name;
                            dest.Timezone = company.Timezone;
                            //TODO add check that mastercompany flag is turned on
                            if (ctx.Options.Items.ContainsKey("DataContext"))
                            {
                                var context = ctx.Options.Items["DataContext"] as DataContext;
                                var isMasterCompany = context.Companies.Any(x => x.MasterCompanyId == company.Id);
                                if (isMasterCompany) dest.IsMasterCompany = true;
                            }
                        }
                    });

                cfg.CreateMap<UserData, UserData>();

                cfg.CreateMap<User, AccountViewModel>().ReverseMap();

                cfg.CreateMap<AccountDetailsEditModel, User>()
                    .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                    .ForMember(dest => dest.ProfilePhotoPath, opt => opt.Condition(src => src.ProfilePhotoFile != null));

                cfg.CreateMap<UserActivity, UserActivityViewModel>()
                    .ForMember(dest => dest.Account, opt => opt.MapFrom(src => src.User));
                cfg.CreateMap<UserActivityViewModel, UserActivity>();

                cfg.CreateMap<RoleSelectionViewModel, RoleSelectionViewModel>();

                cfg.CreateMap<AccountViewModel, RegistrationViewModel>();
                cfg.CreateMap<RegistrationApiModel, RegistrationViewModel>()
                    .ForMember(dest => dest.MixpanelTempId, opt => opt.MapFrom(src => src.AnonymousUserId));

                cfg.CreateMap<RegistrationViewModel, UserCreateModel>()
                    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsAnonymous ? UserStatus.Anonymous : UserStatus.Registered))
                    .AfterMap((src, dest) =>
                     {
                         dest.UserRoles = new List<RoleSelectionViewModel> { new RoleSelectionViewModel { Role = src.Roles, CompanyName = src.CompanyName, CompanyGuid = src.CompanyGuid } };
                     });
                cfg.CreateMap<InviteEditModel, UserCreateModel>()
                    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => UserStatus.Registered))
                    .AfterMap((src, dest) =>
                    {
                        dest.UserRoles = new List<RoleSelectionViewModel> { new RoleSelectionViewModel { Role = src.InviteRole.Role.Value, CompanyName = src.InviteRole.CompanyName, CompanyId = src.InviteRole.CompanyId, Status = RoleStatus.Invited } };
                    });

                cfg.CreateMap<UserCreateModel, User>()
                    .ForMember(dest => dest.UserRoles, opt => opt.Ignore());

                cfg.CreateMap<InviteEditModel, User>()
                    .ForMember(dest => dest.InvitedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => UserStatus.Registered));

                cfg.CreateMap<User, InviteEditModel>()
                    .ForMember(dest => dest.InviteRole, opt => opt.MapFrom(src => src.GetLatestUserRole()))
                    .ForMember(dest => dest.Token, opt => opt.MapFrom(src => src.GetToken(UserTokenType.Invite)));

                cfg.CreateMap<InviteEditApiModel, InviteEditModel>()
                    .ForMember(dest => dest.InviteRole, opt => opt.MapFrom(src => new InviteRoleViewModel { Role = Role.User }));

                cfg.CreateMap<InviteUploadItemModel, InviteEditModel>();

                cfg.CreateMap<Email, EmailSummaryModel>();
                cfg.CreateMap<Email, EmailViewModel>();
                cfg.CreateMap<Email, EmailUnsubscribeModel>();

                UserApiMobileService.AutoMapperConfigure(cfg);
                AccountService.OAuth_AutoMapperConfigure(cfg);
                ProjectConfigure(cfg);
                StripeService.MappingConfigure(cfg);
            });

        }
    }
}
