using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web;
using FoolProof.Core;
using Humanizer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;

namespace ChilliCoreTemplate.Models.Api
{

    public class ManagedAccountApiEditModel : IValidatableObject
    {
        public ManagedAccountApiEditModel()
        {
            Locations = new List<ManagedAccountLocationApiEditModel>();
        }

        public string Id { get; set; }

        public ManagedAccountUserApiEditModel User { get; set; }

        public ManagedAccountCompanyApiEditModel Company { get; set; }

        public List<ManagedAccountLocationApiEditModel> Locations { get; set; }

        public ManagedAccountBankAccountEditModel BankAccount { get; set; }

        public bool HasUser() => !String.IsNullOrEmpty(User?.Email);

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (String.IsNullOrEmpty(Company?.TradingName)) yield return new ValidationResult("Company details is required");
            if (!HasUser()) yield return new ValidationResult("User details is required");
        }
    }


    public class ManagedAccountUserApiEditModel
    {
        [Required, EmailAddressWeb, MaxLength(100)]
        public string Email { get; set; }

        [Required, MaxLength(25)]
        public string FirstName { get; set; }

        [MaxLength(25)]
        public string LastName { get; set; }

        [Required]
        public int? ExternalId { get; set; }

        [Required, MinLength(8), MaxLength(50)]
        public string Password { get; set; }

        public bool IsEmailVerified { get; set; }

    }

    public class ManagedAccountCompanyApiEditModel
    {
        public ManagedAccountCompanyApiEditModel()
        {
        }

        [Required, MaxLength(100)]
        public string TradingName { get; set; }

        [Url]
        public string LogoUrl { get; set; }

        public decimal? Price { get; set; }

        public bool HasPriceAgreement { get; set; }

        public string Street { get; set; }

        public string Suburb { get; set; }

        public string State { get; set; }

        public string Postcode { get; set; }

        [Required, MaxLength(2), MinLength(2)]
        public string Country { get; set; }

        [MaxLength(50)]
        public string Timezone { get; set; }

        [Required]
        public int? ExternalId { get; set; }
    }


    public class ManagedAccountLocationApiEditModel
    {
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string Address { get; set; }

        [Required]
        public string State { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        [Required]
        public bool? IsPublic { get; set; }

        [MaxLength(50)]
        public string Timezone { get; set; }

        public GeoCoordinate Location()
        {
            if (Latitude == null || Longitude == null) return null;
            return new GeoCoordinate(Latitude.Value, Longitude.Value);
        }

        [Required]
        public Guid? ExternalId { get; set; }
    }

}
