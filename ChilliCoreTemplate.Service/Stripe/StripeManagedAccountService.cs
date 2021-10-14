using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Core;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Http;
using Stripe;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ChilliCoreTemplate.Service
{
    public partial class StripeService
    {

        public ServiceResult<Account> ManagedAccount_Get(string id)
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

        #region AddOrUpdate
        public ServiceResult<ManagedAccountViewModel> ManagedAccount_AddOrUpdate(ManagedAccountEditModel model)
        {
            try
            {
                var service = new AccountService(_client);
                Account stripeAccount = null;
                var isNew = String.IsNullOrEmpty(model.Id);

                if (isNew)
                {
                    var account = new AccountCreateOptions
                    {
                        Type = AccountType.Custom,
                        Country = model.Company.Address?.Country,
                        Capabilities = new AccountCapabilitiesOptions { CardPayments = new AccountCapabilitiesCardPaymentsOptions { Requested = true }, Transfers = new AccountCapabilitiesTransfersOptions { Requested = true } },
                        Email = model.UserEmail,
                        BusinessProfile = ManagedAccount_Business(model.Company),
                        BusinessType = model.Company.Type.GetDescription().ToLower()
                    };
                    if (model.HasCompany()) account.Company = ManagedAccount_Company(model.Company);
                    if (model.HasBankAccount()) account.ExternalAccount = ManagedAccount_BankAccount(model.BankAccount, model.Company.Address.Country);
                    if (model.HasTerms()) account.TosAcceptance = ManagedAccount_Terms(model.TermsAgreement);
                    if (model.HasMetaData()) account.Metadata = model.GetMetaData();
                    if (model.HasPayout()) account.Settings = ManagedAccount_Settings(model.Payout);
                    stripeAccount = service.Create(account);
                    model.Id = stripeAccount.Id;
                }
                else
                {
                    var account = new AccountUpdateOptions();
                    if (model.HasTerms()) account.TosAcceptance = ManagedAccount_Terms(model.TermsAgreement);
                    if (model.HasCompany())
                    {
                        account.Company = ManagedAccount_Company(model.Company, model.Id);
                        account.BusinessProfile = ManagedAccount_Business(model.Company);
                    }
                    if (model.HasBankAccount()) account.ExternalAccount = ManagedAccount_BankAccount(model.BankAccount, model.Company.Address.Country);
                    if (model.HasMetaData()) account.Metadata = model.GetMetaData();
                    if (model.HasPayout()) account.Settings = ManagedAccount_Settings(model.Payout);
                    stripeAccount = service.Update(model.Id, account);
                }
                var result = new ManagedAccountViewModel { Id = model.Id, Requirements = stripeAccount.Requirements.EventuallyDue, Pending = stripeAccount.Requirements.PendingVerification, PayoutsEnabled = stripeAccount.PayoutsEnabled };
                if (model.HasRepresentative())
                {
                    var requirements = ManagedAccount_Representative(model.Representative, model.Id);
                    result.Requirements.RemoveAll(x => x.StartsWith("person"));
                    result.Requirements.AddRange(requirements);
                    result.RepresentativeId = model.Representative.Id;

                    if (result.Requirements.Any(x => x.StartsWith("relationship.owner")))  //Now that representative is added update account recognising all executives and owners are now added (1)
                    {
                        var account = new AccountUpdateOptions 
                        {
                            Metadata = new Dictionary<string, string> { { ManagedAccountEditModel.MetadataRepresentative, model.Representative.Id } },
                            Company = new AccountCompanyOptions { OwnersProvided = true, ExecutivesProvided = true } 
                        };
                        stripeAccount = service.Update(model.Id, account);
                        result.Requirements = stripeAccount.Requirements.EventuallyDue;
                    }
                }
                if (stripeAccount.ExternalAccounts != null && stripeAccount.ExternalAccounts.Count() > 0)
                {
                    if (stripeAccount.ExternalAccounts.First() is BankAccount bankaccount)
                    {
                        result.BankAccountId = bankaccount.Id;
                        result.BankAccountDisplay = $"{bankaccount.BankName} - {bankaccount.RoutingNumber} - XXXX{bankaccount.Last4}";
                    }
                }
                return ServiceResult<ManagedAccountViewModel>.AsSuccess(result);
            }
            catch (StripeException ex)
            {
                return ServiceResult<ManagedAccountViewModel>.AsError(ex.Message);
            }
        }

        private AccountTosAcceptanceOptions ManagedAccount_Terms(ManagedAccountTermsEditModel model)
        {
            return new AccountTosAcceptanceOptions
            {
                Date = model.Date,
                Ip = model.IPAddress,
                UserAgent = model.UserAgent
            };
        }

        private AccountBankAccountOptions ManagedAccount_BankAccount(ManagedAccountBankAccountEditModel model, string country)
        {
            var ri = new RegionInfo(country);
            return new AccountBankAccountOptions
            {
                RoutingNumber = model.Bsb.Replace("-", ""),
                AccountNumber = model.AccountNumber,
                Country = country,
                Currency = ri.ISOCurrencySymbol
            };
        }

        private AccountCompanyOptions ManagedAccount_Company(ManagedAccountCompanyEditModel model, string accountId = null)
        {
            return new AccountCompanyOptions
            {
                Name = model.LegalName,
                TaxId = model.TaxId,
                Phone = model.Phone,                
                Address = model.Address == null ? null : new AddressOptions
                {
                    Line1 = model.Address.Street,
                    City = model.Address.Suburb,
                    State = model.Address.State,
                    PostalCode = model.Address.Postcode,
                    Country = model.Address.Country
                },
                DirectorsProvided = true,
                Verification = ManagedAccount_CompanyDocument(model.VerificationFile, accountId)
            };
        }

        private AccountCompanyVerificationOptions ManagedAccount_CompanyDocument(IFormFile file, string accountId)
        {
            if (String.IsNullOrEmpty(accountId) || file == null) return null;
            var fileId = File_Create(new FileCreateOptions
            {
                File = file.OpenReadStream(),
                Purpose = FilePurpose.AdditionalVerification
            }, accountId).Result;

            return new AccountCompanyVerificationOptions
            {
                Document = new AccountCompanyVerificationDocumentOptions
                {
                    Front = fileId
                }
            };
        }

        private AccountBusinessProfileOptions ManagedAccount_Business(ManagedAccountCompanyEditModel model)
        {
            return new AccountBusinessProfileOptions
            {
                Name = model.TradingName,
                Mcc = model.Mcc,
                Url = model.WebSite
            };
        }

        private AccountSettingsOptions ManagedAccount_Settings(ManagedAccountPayoutEditModel model)
        {
            return new AccountSettingsOptions
            {
                Payouts = new AccountSettingsPayoutsOptions
                {
                    Schedule = new AccountSettingsPayoutsScheduleOptions
                    {
                        Interval = model.TransferSchedule.Value.GetDescription().ToLower(),
                        WeeklyAnchor = model.TransferSchedule == TransferSchedule.Weekly ? model.WeekDaySchedule.GetDescription().ToLower() : null,
                        MonthlyAnchor = model.TransferSchedule == TransferSchedule.Monthly ? model.DayOfMonthSchedule.ToString() : null
                    },
                    StatementDescriptor = "APPRECI"
                }
            };
        }

        private List<string> ManagedAccount_Representative(ManagedAccountRepresentativeEditModel representative, string accountId)
        {
            var service = new PersonService(_client);
            Person person = null;

            if (String.IsNullOrEmpty(representative.Id))
            {
                var options = new PersonCreateOptions
                {
                    FirstName = representative.FirstName,
                    LastName = representative.LastName,
                    Phone = CommonLibrary.PhoneFormat(representative.Phone, representative.Address.Country),
                    Email = representative.Email,
                    Dob = new DobOptions { Day = representative.DateOfBirth.Value.Day, Month = representative.DateOfBirth.Value.Month, Year = representative.DateOfBirth.Value.Year },
                    Address = new AddressOptions
                    {
                        Line1 = representative.Address.Street,
                        City = representative.Address.Suburb,
                        State = representative.Address.State,
                        PostalCode = representative.Address.Postcode,
                        Country = representative.Address.Country
                    },
                    Relationship = new PersonRelationshipOptions { Representative = true, Executive = true, Owner = representative.IsOwner, Title = representative.Role }
                };
                if (representative.PhotoId != null || representative.AdditionalVerification != null)
                {
                    options.Verification = new PersonVerificationOptions 
                    { 
                        Document = ManagedAccount_RepresentativeDocument(representative.PhotoId, FilePurpose.IdentityDocument, accountId),
                        //AdditionalDocument = ManagedAccount_RepresentativeDocument(representative.AdditionalVerification, FilePurpose.IdentityDocument, accountId)
                    };
                }
                person = service.Create(accountId, options);
                representative.Id = person.Id;
            }
            else
            {
                var options = new PersonUpdateOptions
                {
                    FirstName = representative.FirstName,
                    LastName = representative.LastName,
                    Phone = CommonLibrary.PhoneFormat(representative.Phone, representative.Address.Country),
                    Email = representative.Email,
                    Dob = new DobOptions { Day = representative.DateOfBirth.Value.Day, Month = representative.DateOfBirth.Value.Month, Year = representative.DateOfBirth.Value.Year },
                    Address = new AddressOptions
                    {
                        Line1 = representative.Address.Street,
                        City = representative.Address.Suburb,
                        State = representative.Address.State,
                        PostalCode = representative.Address.Postcode,
                        Country = representative.Address.Country
                    },
                    Relationship = new PersonRelationshipOptions { Representative = true, Executive = true, Owner = representative.IsOwner, Title = representative.Role }
                };
                if (representative.PhotoId != null || representative.AdditionalVerification != null)
                {
                    options.Verification = new PersonVerificationOptions
                    {
                        Document = ManagedAccount_RepresentativeDocument(representative.PhotoId, FilePurpose.IdentityDocument, accountId),
                        //AdditionalDocument = ManagedAccount_RepresentativeDocument(representative.AdditionalVerification, FilePurpose.AdditionalVerification, accountId)
                    };
                }
                person = service.Update(accountId, representative.Id, options);
            }
            return person.Requirements.EventuallyDue;
        }

        private PersonVerificationDocumentOptions ManagedAccount_RepresentativeDocument(ManagedAccountDocumentEditModel document, string purpose, string accountId)
        {
            if (document.FrontFile != null)
            {
                document.Front = File_Create(new FileCreateOptions
                {
                    File = document.FrontFile.OpenReadStream(),
                    Purpose = purpose                    
                }, accountId).Result;
            }

            return document == null ? null :
            new PersonVerificationDocumentOptions
            {
                Front = document.Front,
                Back = document.Back
            };
        }

        #endregion


        public ServiceResult ManagedAccount_Delete(string id)
        {
            try
            {
                var service = new AccountService(_client);
                var response = service.Delete(id);
                return ServiceResult.AsSuccess();
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult.AsError(ex.Message);
            }
        }
    }
}
