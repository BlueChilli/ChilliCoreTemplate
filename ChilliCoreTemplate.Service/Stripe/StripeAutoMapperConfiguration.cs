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

            //cfg.CreateMap<StripePayoutDetail, Data.Payout>()
            //    .ForMember(dest => dest.Id, opt => opt.Ignore())
            //    .ForMember(dest => dest.PayoutId, opt => opt.MapFrom(src => src.Id))
            //    .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.UtcNow));

        }
    }
}
