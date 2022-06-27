using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Core.Extensions;
using Stripe;
using System;

namespace ChilliCoreTemplate.Service
{
    public partial class StripeService
    {

        public ServiceResult<Account> Account_Get(string id)
        {
            try
            {
                var service = new AccountService(_client);
                var response = service.Get(id);
                return ServiceResult<Account>.AsSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<Account>.AsError(ex.Message);
            }
        }

        public ServiceResult<Account> Account_Create(Company company, UserData user, bool isIndividual)
        {
            try
            {
                var service = new AccountService(_client);
                var account = new AccountCreateOptions
                {
                    Country = company.Country?.ToString(),
                    Email = user.Email,
                    Company = new AccountCompanyOptions
                    {
                        //TaxId = company.TaxId,
                        Address = new AddressOptions
                        {
                            Line1 = company.Street,
                            City = company.Suburb,
                            PostalCode = company.Postcode,
                            State = company.State,
                            Country = company.Country?.ToString()
                        }
                    },
                    BusinessProfile = new AccountBusinessProfileOptions
                    {
                        Name = company.Name,
                        ProductDescription = "Digital platform reseller - property documentation management", //TODO set as needed
                        //Mcc = "5815",
                        Url = company.Website
                    },
                    BusinessType = isIndividual ? "individual" : "company", //TODO determine if company/individual
                    Type = "standard"
                };
                if (isIndividual)
                {
                    account.Individual = new AccountIndividualOptions
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email
                        //Phone = CommonLibrary.PhoneFormat(user.Phone, user.Address.Region)
                    };
                }
                var response = service.Create(account);
                return ServiceResult<Account>.AsSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<Account>.AsError(ex.Message);
            }
        }

        public ServiceResult<AccountLink> Account_Link(string id, string refreshUrl, string returnUrl, string type = "account_onboarding")
        {
            try
            {
                var service = new AccountLinkService(_client);
                var response = service.Create(new AccountLinkCreateOptions
                {
                    Account = id,
                    Type = type,
                    RefreshUrl = refreshUrl,
                    ReturnUrl = returnUrl
                });
                return ServiceResult<AccountLink>.AsSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<AccountLink>.AsError(ex.Message);
            }
        }


    }
}
