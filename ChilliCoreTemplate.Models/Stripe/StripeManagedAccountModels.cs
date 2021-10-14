using AutoMapper.Mappers;
using ChilliSource.Cloud.Web;
using ChilliSource.Cloud.Web.MVC;
using ChilliSource.Core.Extensions;
using FoolProof.Core;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Policy;

namespace ChilliCoreTemplate.Models
{
    public class ManagedAccountEditModel
    {
        public ManagedAccountEditModel()
        {
            Errors = new List<string>();
            Requirements = new List<string>();
            RequirementStatus = ManagedAccountRequirement.Terms;
        }

        public const string MetadataRepresentative = "Representative";

        public string Id { get; set; }

        public bool IsNew => String.IsNullOrEmpty(Id);
        [RequiredIfTrue("IsNew")]
        public string UserEmail { get; set; }

        public ManagedAccountCompanyEditModel Company { get; set; }

        public ManagedAccountBankAccountEditModel BankAccount { get; set; }

        public ManagedAccountRepresentativeEditModel Representative { get; set; }

        public ManagedAccountTermsEditModel TermsAgreement { get; set; }

        public ManagedAccountPayoutEditModel Payout { get; set; }

        public bool HasBankAccount() => !String.IsNullOrEmpty(BankAccount?.AccountNumber) && !String.IsNullOrEmpty(Company?.Address?.Country);

        public bool HasCompany() => !String.IsNullOrEmpty(Company?.LegalName);

        public bool HasMetaData() => Payout?.PayoutRate != null;
        public Dictionary<string, string> GetMetaData()
        {
            var result = new Dictionary<string, string>();

            foreach (var item in Payout.PayoutRate)
            {
                result.Add(item.PriceId, item.Rate.ToString());
            }

            return result;
        }

        public bool HasTerms() => TermsAgreement?.Terms ?? false;

        public bool HasPayout() => Payout?.TransferSchedule != null;

        public bool HasRepresentative() => !String.IsNullOrEmpty(Representative?.FirstName);

        public ManagedAccountRequirement RequirementStatus { get; set; }

        public List<string> Errors { get; set; }

        public List<string> Requirements { get; set; }

        public bool IsActive(ManagedAccountRequirement status)
        {
            return status == RequirementStatus || (status == ManagedAccountRequirement.Company && RequirementStatus == ManagedAccountRequirement.Complete);
        }

        public bool IsDisabled(ManagedAccountRequirement status)
        {
            return status > RequirementStatus;
        }

        public HtmlString SetUpIcon(ManagedAccountRequirement status)
        {
            if (RequirementStatus == ManagedAccountRequirement.Complete) return HtmlString.Empty;
            var color = "";

            switch(status)
            {
                case ManagedAccountRequirement.Company:
                    if (!HasCompany()) return HtmlString.Empty;
                    color = Requirements.Any(x => x.StartsWith("business") || x.StartsWith("company")) ? "warning" : "success";
                    break;
                case ManagedAccountRequirement.Payment:
                    if (!HasBankAccount() && String.IsNullOrEmpty(BankAccount?.Display)) return HtmlString.Empty;
                    color = Requirements.Any(x => x.StartsWith("external_account") || !Payout.TransfersEnabled) ? "warning" : "success";
                    break;
                case ManagedAccountRequirement.Representative:
                    if (!HasRepresentative() || Representative.VerificationStatus == PersonVerificationStatus.None) return HtmlString.Empty;
                    color = Representative.VerificationStatus.GetData<string>("Label");
                    break;
            }

            if (color != "") return new HtmlString($"<i class='far fa-check-circle text-{color}'></i>");

            return HtmlString.Empty;
        }

        public void SetCompletionStatus(List<string> requirements)
        {
            if (requirements.Any(x => x.StartsWith("tos_acceptance"))) RequirementStatus = ManagedAccountRequirement.Terms;
            else if (requirements.Any(x => x.StartsWith("business") || x.StartsWith("company"))) RequirementStatus = ManagedAccountRequirement.Company;
            else if (requirements.Any(x => x.StartsWith("external_account"))) RequirementStatus = ManagedAccountRequirement.Payment;
            else if (requirements.Any(x => x.StartsWith("relationship.representative") || x.StartsWith("person"))) RequirementStatus = ManagedAccountRequirement.Representative;
            else RequirementStatus = ManagedAccountRequirement.Complete;

            var addressCount = requirements.Where(x => x.StartsWith("company.address")).Count();
            if (addressCount > 0 && addressCount < 4) Errors.Add("Only full addresses are accepted");
        }
    }

    public class ManagedAccountViewModel
    {
        public string Id { get; set; }
        public string BankAccountId { get; set; }
        public string BankAccountDisplay { get; set; }
        public string RepresentativeId { get; set; }
        public List<string> Requirements { get; set; }
        public List<string> Pending { get; set; }
        public bool PayoutsEnabled { get; set; }

        public bool IsSetup() => !Requirements.Any() && !Pending.Any() && PayoutsEnabled;

    }

    public class ManagedAccountCompanyEditModel : IValidatableObject
    {
        public ManagedAccountCompanyEditModel()
        {
            Type = MangagedAccountCompanyType.Company;
        }

        public bool IsRequired { get; set; }    //Used as IFormFile caused model to be created during binding even though no data passed

        public MangagedAccountCompanyType Type { get; set; }

        [RequiredIfTrue("IsRequired", ErrorMessage = "The Legal name field is required."), MaxLength(100), HelpText("Your registered company name")]
        public string LegalName { get; set; }

        [RequiredIfTrue("IsRequired", ErrorMessage = "The Name field is required."), MaxLength(100), DisplayName("Name"), HelpText("Your trading name")]
        public string TradingName { get; set; }

        [MaxLength(100), DisplayName("Number"), HelpText("eg ABN")]
        public string TaxId { get; set; }
        public bool TaxIdProvided { get; set; }

        /// <summary>
        /// https://stripe.com/docs/connect/setting-mcc#list
        /// Eating Places, Restaurants	5812
        /// Fast Food Restaurants	5814
        /// Miscellaneous Food Stores - Convenience Stores and Specialty Markets	5499
        /// Package Stores-Beer, Wine, and Liquor	5921
        /// Miscellaneous Specialty Retail	5999
        /// </summary>
        [RequiredIfTrue("IsRequired", ErrorMessage = "The Classification field is required."), DisplayName("Classification"), EmptyItem]
        public string Mcc { get; set; }
        public SelectList MccList => new Dictionary<string, string> { { "5812", "Cafe or restaurant" }, { "5814", "Fast food restaurant" }, { "5499", "Food store or market" }, { "5921", "Liquor store" }, { "5999", "Other" } }.ToSelectList(v => v.Key, t => t.Value);

        public ManagedAccountAddressEditModel Address { get; set; }

        [RequiredIfTrue("IsRequired", ErrorMessage = "The Phone field is required."), MaxLength(20)]
        public string Phone { get; set; }

        [RequiredIfTrue("IsRequired", ErrorMessage = "The Web site field is required."), Url(ErrorMessage = Constants.UrlErrorMessage), MaxLength(100)]
        public string WebSite { get; set; }

        [EmailAddressWeb]
        public string Email { get; set; }

        public int? ExternalId { get; set; }

        [DisplayName("Logo")]
        [FileMaxSize(8 * 1024 * 1024), FileMinSize]
        [HttpPostedFileExtensions(allowedExtensions: "jpg, jpeg, png, gif")]
        public IFormFile LogoFile { get; set; }
        public string LogoPath { get; set; }

        [DisplayName("Proof of Legal Entity"), HelpText("The information in this document (pdf)  must include business name, business address, and company number as provided")]
        [FileMaxSize(8 * 1024 * 1024), FileMinSize]
        [HttpPostedFileExtensions(allowedExtensions: "pdf")]
        public IFormFile VerificationFile { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsRequired && !TaxIdProvided && String.IsNullOrEmpty(TaxId)) yield return new ValidationResult("The Tax number field is required.");
        }
    }

    public class ManagedAccountAddressEditModel
    {
        [Required, MaxLength(100)]
        public string Address { get; set; }

        public string Street { get; set; }

        public string Suburb { get; set; }

        public string State { get; set; }

        public string Postcode { get; set; }

        [MaxLength(2), MinLength(2)]
        public string Country { get; set; }

        [MustBeTrue(ErrorMessage = "An address must be selected from the drop down")]
        public bool AddressSelected { get; set; }
        public string AddressComponents { get; set; }

        [MaxLength(50)]
        public string Timezone { get; set; }

    }

    public class ManagedAccountBankAccountEditModel
    {

        [DisplayName("BSB")]
        public string Bsb { get; set; }

        [DisplayName("Account Number")]
        public string AccountNumber { get; set; }

        [ReadOnly(true)]
        public string Display { get; set; }
    }

    public class ManagedAccountRepresentativeEditModel
    {
        public string Id { get; set; }

        [CheckBox, HelpText("Is the representative the owner (25% or more) of the company?")]
        public bool IsOwner { get; set; }

        [Required, MaxLength(50)]
        public string Role { get; set; }

        [Required, MaxLength(25)]
        public string FirstName { get; set; }

        [Required, MaxLength(25)]
        public string LastName { get; set; }

        [Required, DateFormat, HelpText("Birthdate is required to help with the verification of the representative identity")]
        public DateTime? DateOfBirth { get; set; }

        [Required, EmailAddressWeb]
        public string Email { get; set; }

        [Required, MaxLength(20)]
        public string Phone { get; set; }

        public ManagedAccountAddressEditModel Address { get; set; }

        public ManagedAccountDocumentEditModel PhotoId { get; set; }

        public ManagedAccountDocumentEditModel AdditionalVerification { get; set; }

        public PersonVerificationStatus VerificationStatus { get; set; }

    }

    public class ManagedAccountDocumentEditModel
    {
        public string Front { get; set; }

        [FileMaxSize(8 * 1024 * 1024), FileMinSize]
        [HttpPostedFileExtensions(allowedExtensions: "jpg, jpeg, png, gif")]
        public IFormFile FrontFile { get; set; }

        public string Back { get; set; }

        [FileMaxSize(8 * 1024 * 1024), FileMinSize]
        [HttpPostedFileExtensions(allowedExtensions: "jpg, jpeg, png, gif")]
        public IFormFile BackFile { get; set; }
    }

    public class ManagedAccountFileUploadModel
    {
        public string Id { get; set; }

        [FileMaxSize(9 * 1024 * 1024), FileMinSize]
        [HttpPostedFileExtensions(allowedExtensions: "jpg, jpeg, png")]
        public IFormFile File { get; set; }

        public ManagedAccountDocumentPurpose Purpose { get; set; }

    }

    /// <summary>
    /// https://stripe.com/docs/connect/updating-accounts#tos-acceptance
    /// </summary>
    public class ManagedAccountTermsEditModel
    {

        [CheckBox(Label = "I agree to the Appreci <a href=\"https://appreci.io/terms/\" target=\"blank\">Terms of Service</a>")]
        public bool Terms { get; set; }

        public DateTime Date { get; set; }

        public string IPAddress { get; set; }

        public string UserAgent { get; set; }

        public void SetTerms(string userAgent, string ipAddress)
        {
            Date = DateTime.UtcNow;
            UserAgent = userAgent;
            IPAddress = ipAddress; 
        }
    }

    public class ManagedAccountPayoutEditModel
    {
        public ManagedAccountPayoutEditModel()
        {
            WeekDaySchedule = DayOfWeek.Monday;
            DayOfMonthSchedule = 1;
        }

        public List<ManagedAccountPayoutRate> PayoutRate { get; set; }

        /// <summary>
        /// Default is daily
        /// </summary>
        public TransferSchedule? TransferSchedule { get; set; }

        public DayOfWeek WeekDaySchedule { get; set; }

        public long DayOfMonthSchedule { get; set; }

        public bool TransfersEnabled { get; set; }
    }

    public class ManagedAccountPayoutRate
    {
        public string PriceId { get; set; }

        public decimal Rate { get; set; }
    }

    public class ManagedAccountCompleteEmailModel
    {
        public CompanyViewModel Company { get; set; }
        public bool HasTransfersPending { get; set; }
        public string Email { get; set; }
    }

    public enum TransferSchedule
    {
        Daily = 1,
        Weekly,
        Monthly
    }

    public enum MangagedAccountCompanyType
    {
        Company = 1,
        Individual,
        [Description("non_profit")]
        NonProfit
    }

    public enum ManagedAccountDocumentPurpose
    {
        IdentityDocument = 1,
        AdditionalVerification
    }

    public enum ManagedAccountRequirement
    {
        Terms,
        Company,
        Payment,
        Representative,
        Complete
    }

    public enum PersonVerificationStatus
    {
        None,
        [Data("Label", "error")]
        Unverified = 1,
        [Data("Label", "warning")]
        Pending,
        [Data("Label", "success")]
        Verified
    }
}
