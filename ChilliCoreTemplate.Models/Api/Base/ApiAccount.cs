using ChilliSource.Core.Extensions;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web.MVC;
using ChilliCoreTemplate.Models.EmailAccount;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using ChilliSource.Cloud.Web;
using HybridModelBinding;

namespace ChilliCoreTemplate.Models.Api
{
    public class NewSessionByPhoneApiModel
    {
        [Required, PhoneNumber("AU")]
        public string Phone { get; set; }

        [Required, StringLength(200)]
        public string VerificationToken { get; set; }

        [Required, StringLength(10)]
        public string VerificationCode { get; set; }

        [StringLength(100)]
        public string DeviceId { get; set; }
    }

    public class RegistrationApiModel
    {
        [Required, MaxLength(25)]
        public string FirstName { get; set; }

        [Required, MaxLength(25)]
        public string LastName { get; set; }

        [Required, EmailAddressWeb, MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(50)]
        public string CompanyName { get; set; }

        public Guid? CompanyGuid { get; set; }

        [Required, DataType(DataType.Password), MinLength(6), MaxLength(50)]
        public string Password { get; set; }

        [MustBeTrue(ErrorMessage = "Please accept terms and conditions")]
        public bool AcceptTermsConditions { get; set; }

        /// <summary>
        /// If true user is unknown and won't be sent emails. 
        /// When user has change details api called this flag can be set to false, which will trigger welcome email
        /// </summary>
        public bool IsAnonymous { get; set; }

        /// <summary>
        /// eg Id used to track actions in mixpanel before account created. Not related to IsAnonymous.
        /// </summary>
        public Guid? AnonymousUserId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var result = new List<ValidationResult>();

            if (Email == Password)
            {
                result.Add(new ValidationResult("Password can not be the same as your email address", new string[] { "Password" }));
            }

            return result;
        }

        public Role GetRole()
        {
            if (!String.IsNullOrEmpty(CompanyName)) return Role.CompanyAdmin;
            else if (CompanyGuid.HasValue) return Role.CompanyUser;
            return Role.User;
        }

    }


    public class PhoneRegistrationApiModel
    {
        [FileMaxSize(8 * 1024 * 1024), FileMinSize]
        [HttpPostedFileExtensions(allowedExtensions: "jpg, jpeg, png, gif")]
        public IFormFile ProfilePhotoFile { get; set; }

        [Required, MaxLength(25)]
        public string FirstName { get; set; }

        [Required, MaxLength(25)]
        public string LastName { get; set; }

        [Required, PhoneNumber("AU", PhoneNumbers.PhoneNumberType.MOBILE, ErrorMessage = "Please enter a valid Australian mobile number"), MaxLength(15)]
        public string Phone { get { return _Phone; } set { _Phone = ValidPhone(value); } }
        private string _Phone;

        [EmailAddressWeb, MaxLength(100)]
        public string Email { get; set; }

        public Guid? AnonymousUserId { get; set; }   //For example id used to track in mixpanel before account created

        private string ValidPhone(string p)
        {
            if (String.IsNullOrEmpty(p)) return null;
            if (p.StartsWith("+"))
            {
                return p[1..];
            }
            return p;
        }
    }

    #region Mobile

    public class PushTokenRegistrationApiModel
    {
        [Required]
        public string DeviceId { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public PushNotificationProvider? Provider { get; set; }

        [Required]
        public PushNotificationAppId? AppId { get; set; }

    }

    public class RegistrationMobileModel
    {
        [Required]
        public Guid AnonymousUserId { get; set; }   //For example id used to track in mixpanel before account created

        [Required, MaxLength(25)]
        public string FirstName { get; set; }

        [Required, MaxLength(25)]
        public string LastName { get; set; }

        [Required, EmailAddressWeb, MaxLength(100)]
        public string Email { get; set; }

        [Required, PhoneNumber("AU")]
        public string Phone { get; set; }

        [MinLength(4), MaxLength(4), Numeric]
        public string Pin { get; set; } //Add required if using PIN for mobile authentication
    }

    public class LoginResultMobileModel
    {
        public int UserId { get; set; }

        public string UserKey { get; set; }

        public bool MustChangePassword { get; set; }
        public bool IsVerified { get; set; }
        public bool IsInvited { get; set; }
    }

    public class ChangePinViewModel
    {
        [Required, MinLength(4), MaxLength(4), Numeric]
        public string CurrentPin { get; set; }

        [Required, MinLength(4), MaxLength(4), Numeric]
        public string NewPin { get; set; }
    }

    #endregion

    public class ApiGetAccountRequest
    {
        [Required]
        public int? UserId { get; set; }
    }

    public class TokenEditApiModel
    {
        [Required, MaxLength(100), EmailAddressWeb, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        public UserTokenType? Type { get; set; }
    }

    public class InviteEditApiModel
    {
        [MaxLength(25)]
        public string FirstName { get; set; }

        [MaxLength(25)]
        public string LastName { get; set; }

        public string FullName => String.IsNullOrEmpty(FirstName) && String.IsNullOrEmpty(LastName) ? null : $"{FirstName} {LastName}".Trim();

        [Required, MaxLength(100), EmailAddressWeb, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

    }

    public class UserAccountApiModel : IInterceptApiModel
    {
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }

        public virtual string FullName { get { return String.Concat(FirstName, " ", LastName).Trim(); } }

        public virtual string Email { get; set; }

        public virtual string Phone { get; set; }

        public virtual UserStatus Status { get; set; }

        public List<UserRoleApiModel> UserRoles { get; set; }

        public virtual List<string> Roles { get; set; }

        //TODO: transform to full address when mapping object
        public virtual string ProfilePhotoPath { get; set; }

        [JsonIgnore]
        public List<string> PopulatedMembers { get; set; }
    }

    public class UserRoleApiModel
    {
        public Role Role { get; set; }
        public int? CompanyId { get; set; }
    }

    public class AccountTokenApiModel
    {
        [Required]
        [StringLength(100)]
        public virtual string Email { get; set; }

        public virtual string Token { get; set; }
    }

    [HybridBindClass(defaultBindingOrder: new[] { Source.Route, Source.Body, Source.Form })]
    public class PatchAccountTokenApiModel : EmailTokenModel
    {
        [MaxLength(25)]
        public virtual string FirstName { get; set; }

        [MaxLength(25)]
        public virtual string LastName { get; set; }

        [DataType(DataType.Password), MinLength(6), MaxLength(50)]
        public virtual string Password { get; set; }
    }

    public class PersistUserAccountApiModel
    {
        [JsonIgnore] //From session
        public int Id { get; set; }

        [Required, MaxLength(25)]
        public string FirstName { get; set; }

        [Required, MaxLength(25)]
        public string LastName { get; set; }

        [Required, EmailAddressWeb, MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        [FileMaxSize(8 * 1024 * 1024), FileMinSize]
        [HttpPostedFileExtensions(allowedExtensions: "jpg, jpeg, png, gif")]
        public IFormFile ProfilePhotoFile { get; set; }
    }

    public class PatchAccountApiModel : IValidatableObject
    {
        [JsonIgnore] //From uri
        public int Id { get; set; }

        [DataType(DataType.Password), MaxLength(50)]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password), MinLength(6), MaxLength(50)]
        public string Password { get; set; }
        public bool PasswordSpecified { get; set; }

        [EmailAddressWeb, MaxLength(100)]
        public string Email { get; set; }
        public bool EmailSpecified { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }
        public bool PhoneSpecified { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            this.Email = this.Email?.Trim();
            this.Password = this.Password?.Trim();
            this.CurrentPassword = this.CurrentPassword?.Trim();

            if (this.EmailSpecified && string.IsNullOrEmpty(this.Email))
            {
                yield return new ValidationResult("The email field is required.", new string[] { "Email" });
            }

            if (this.PasswordSpecified)
            {
                if (string.IsNullOrEmpty(this.CurrentPassword))
                {
                    yield return new ValidationResult("The current password field is required.", new string[] { "CurrentPassword" });
                }

                if (string.IsNullOrEmpty(this.Password))
                {
                    yield return new ValidationResult("The new password field is required", new string[] { "Password" });
                }
            }
        }
    }

    public class DeleteUserApiModel : IValidatableObject
    {
        [JsonIgnore]
        public int Id { get; set; }

        [Required]
        public string Confirm { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Confirm.ToUpperInvariant() != "DELETE") yield return new ValidationResult("Enter DELETE to confirm deactivation of account", new string[] { "Confirm" });
        }
    }

}
