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
        public class StripeServiceAutoMapperConfig : Profile
        {
            public StripeServiceAutoMapperConfig()
            {
                CreateMap<User, StripeCustomerEditModel>()
                    .ForMember(x => x.Id, opt => opt.MapFrom(src => src.StripeId))
                    .ForMember(x => x.Name, opt => opt.MapFrom(src => src.FullName));
            }
        }
    }
}
