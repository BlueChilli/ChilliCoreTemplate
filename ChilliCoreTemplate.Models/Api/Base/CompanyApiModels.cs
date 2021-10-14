using ChilliSource.Cloud.Web;
using FoolProof.Core;
using HybridModelBinding;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ChilliCoreTemplate.Models.Api
{
    public class CompanyEditApiModel
    {
        [Required, StringLength(100)]
        public string Name { get; set; }

        public string LogoFileBase64 { get; set; }

        [RequiredIfNotEmptyAttribute("LogoFileBase64")]
        public string LogoFileName { get; set; }

    }

    public class CompanyApiModel
    {
        public string Name { get; set; }

        public string LogoPath { get; set; }

    }

    public class CompanyUserApiModel
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public UserStatus Status { get; set; }

        public DateTime? LastLoginOn { get; set; }

    }

    public class CompanyUserEditApiModel
    {
        [HybridBindProperty(Source.Route, "id")]
        [JsonIgnore]
        public int Id { get; set; }

        [MaxLength(25)]
        public string FirstName { get; set; }

        [MaxLength(25)]
        public string LastName { get; set; }

        [Required, MaxLength(100), EmailAddressWeb]
        public string Email { get; set; }

    }

}
