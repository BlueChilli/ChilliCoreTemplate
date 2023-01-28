using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Models.Stripe;
using AutoMapper;
using ChilliSource.Cloud.Core;
using ChilliSource.Core.Extensions;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using ChilliCoreTemplate.Data.EmailAccount;

namespace ChilliCoreTemplate.Service
{
    public partial class StripeService
    {
        internal static void MappingConfigure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<User, StripeCustomerEditModel>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.StripeId))
                .ForMember(x => x.Description, opt => opt.MapFrom(src => src.FullName));

            cfg.CreateMap<ManagedAccountApiEditModel, RegistrationViewModel>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.TradingName))
                //.ForMember(dest => dest.CompanyExternalId, opt => opt.MapFrom(src => src.Company.ExternalId))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
                //.ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.User.ExternalId))
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => Role.CompanyAdmin))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.User.Password));

            cfg.CreateMap<ManagedAccountApiEditModel, ManagedAccountEditModel>();
            cfg.CreateMap<ManagedAccountCompanyApiEditModel, ManagedAccountCompanyEditModel>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => Mapper.Map<ManagedAccountAddressEditModel>(src)));
            cfg.CreateMap<ManagedAccountCompanyApiEditModel, ManagedAccountAddressEditModel>()
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country ?? Constants.DefaultCountry));
    
            cfg.CreateMap<Account, ManagedAccountEditModel>()
                .ForMember(dest => dest.Requirements, opt => opt.MapFrom(src => src.Requirements.EventuallyDue))
                .ForMember(dest => dest.BankAccount, opt => opt.MapFrom(src => src.ExternalAccounts.FirstOrDefault() as Stripe.BankAccount ?? new Stripe.BankAccount()))
                .ForMember(dest => dest.Payout, opt => opt.MapFrom(src => src.Settings.Payouts.Schedule))
                .ForMember(dest => dest.Representative, opt => opt.MapFrom(src => new ManagedAccountRepresentativeEditModel { Id = src.Metadata.GetValueOrDefault(ManagedAccountEditModel.MetadataRepresentative) }))
                .AfterMap((src, dest) =>
                {
                    dest.Company = Mapper.Map<ManagedAccountCompanyEditModel>(src.BusinessProfile);
                    Mapper.Map(src.Company, dest.Company);
                    dest.SetCompletionStatus(src.Requirements.EventuallyDue);
                    dest.Payout.TransfersEnabled = src.PayoutsEnabled;
                    if (src.Requirements.Errors.Any())
                    {
                        dest.Errors = src.Requirements.Errors.DistinctBy(x => x.Code).Select(x => x.Reason).ToList();
                    }
                });

            cfg.CreateMap<AccountBusinessProfile, ManagedAccountCompanyEditModel>()
                .ForMember(dest => dest.TradingName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.WebSite, opt => opt.MapFrom(src => src.Url));
            cfg.CreateMap<AccountCompany, ManagedAccountCompanyEditModel>()
                .ForMember(dest => dest.LegalName, opt => opt.MapFrom(src => src.Name));
            cfg.CreateMap<BankAccount, ManagedAccountBankAccountEditModel>()
                .ForMember(dest => dest.Bsb, opt => opt.MapFrom(src => src.RoutingNumber.ToNumeric()))
                .ForMember(dest => dest.Display, opt => opt.MapFrom(src => String.IsNullOrEmpty(src.BankName) ? null : $"{src.BankName} - {src.RoutingNumber} - XXXX{src.Last4}"));
            cfg.CreateMap<Address, ManagedAccountAddressEditModel>()
                .ForMember(dest => dest.Street, opt => opt.MapFrom(src => src.Line1))
                .ForMember(dest => dest.Suburb, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.Postcode, opt => opt.MapFrom(src => src.PostalCode))
                .AfterMap((src, dest) =>
                {
                    if (!String.IsNullOrEmpty(src.Line1))
                    {
                        dest.AddressSelected = true;
                        dest.Address = $"{src.Line1}, {src.City} {src.State} {src.PostalCode}, {src.Country}";
                    }
                });
            cfg.CreateMap<AccountSettingsPayoutsSchedule, ManagedAccountPayoutEditModel>()
                .AfterMap((src, dest) =>
                 {
                     dest.DayOfMonthSchedule = src.MonthlyAnchor == 0 ? 1 : src.MonthlyAnchor;
                     dest.WeekDaySchedule = EnumHelper.Parse<DayOfWeek>(src.WeeklyAnchor);
                     dest.TransferSchedule = EnumHelper.Parse<TransferSchedule>(src.Interval);
                 });
            cfg.CreateMap<Person, ManagedAccountRepresentativeEditModel>()
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.Dob == null ? null : new DateTime((int)src.Dob.Year.Value, (int)src.Dob.Month.Value, (int)src.Dob.Day.Value).ToNullable<DateTime>()))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Relationship.Title))
                .ForMember(dest => dest.IsOwner, opt => opt.MapFrom(src => src.Relationship.Owner))
                .ForMember(dest => dest.VerificationStatus, opt => opt.MapFrom(src => EnumHelper.Parse<PersonVerificationStatus>(src.Verification.Status)));

            //cfg.CreateMap<StripePayoutDetail, Data.Payout>()
            //    .ForMember(dest => dest.Id, opt => opt.Ignore())
            //    .ForMember(dest => dest.PayoutId, opt => opt.MapFrom(src => src.Id))
            //    .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.UtcNow));

        }
    }
}
