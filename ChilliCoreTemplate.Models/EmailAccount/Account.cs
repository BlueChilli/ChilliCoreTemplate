using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web.MVC;
using ChilliSource.Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Web;
using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Http; using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using ChilliCoreTemplate.Models.Api;
using ChilliSource.Cloud.Web;

namespace ChilliCoreTemplate.Models.EmailAccount
{
    public class AccountViewModel
    {
        public int Id { get; set; }

        [Required, MaxLength(25)]
        public string FirstName { get; set; }

        [Required, MaxLength(25)]
        public string LastName { get; set; }

        [Required, MaxLength(100), EmailAddressWeb, DisplayName("Username")]
        public string Email { get; set; }

        public string Phone { get; set; }

        public string ShortName { get { return String.IsNullOrWhiteSpace(FullName) ? "" : FirstName + " " + LastName?.ToUpper()[0] + "."; } }
        public string FullName { get; set; }

        public string ProfilePhotoPath { get; set; }

        public List<UserRoleModel> UserRoles { get; set; }
        public bool HasRole(Role role) => UserRoles.Any(r => (r.Role & role) > 0);

        public DateTime CreatedDate { get; set; }

        public DateTime? LastLoginDate { get; set; }

        public DateTime? ActivatedDate { get; set; }

        public DateTime? LastPasswordChangedDate { get; set; }

        public DateTime? InvitedDate { get; set; }

        public UserStatus Status { get; set; }
    }

    public class RoleSelectionViewModel
    {
        [RemoveItem(EnumValue = Role.Administrator)]
        public Role Role { get; set; }
        public int? CompanyId { get; set; }
        public string CompanyName { get; set; }
        public Guid? CompanyGuid { get; set; }
    }

    public class RegistrationViewModel : ProcessResultsViewModel, IValidatableObject
    {
        public RegistrationViewModel()
        {
            AcceptTermsConditions = true;
        }

        public int Id { get; set; }

        [MaxLength(50), Placeholder("Company name")]
        public string CompanyName { get; set; }

        public Guid? CompanyGuid { get; set; }

        [Required, MaxLength(25), Placeholder("Your first name")]
        public string FirstName { get; set; }

        [Required, MaxLength(25), Placeholder("Your last name")]
        public string LastName { get; set; }

        [Required, EmailAddressWeb, MaxLength(100), Placeholder("Your email address")]
        public string Email { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        [Required, DataType(DataType.Password), Placeholder("Your password"), MinLength(6), MaxLength(50)]
        public string Password { get; set; }

        [MustBeTrue(ErrorMessage = "Please accept terms and conditions")]
        public bool AcceptTermsConditions { get; set; }

        public Role Roles { get; set; }

        public Guid? MixpanelTempId { get; set; }

        /// <summary>
        /// If true user is unknown and won't be sent emails. 
        /// When user has change details api called this flag can be set to false, which will trigger welcome email
        /// </summary>
        public bool IsAnonymous { get; set; }

        [FileMaxSize(8 * 1024 * 1024), FileMinSize]
        [HttpPostedFileExtensions(allowedExtensions: "jpg, jpeg, png, gif")]
        public IFormFile ProfilePhotoFile { get; set; }

        public bool IsApi { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var result = new List<ValidationResult>();

            if (Email == Password)
            {
                result.Add(new ValidationResult("Password can not be the same as your email address", new string[] { "Password" }));
            }

            if (Roles.IsCompanyRole() && String.IsNullOrEmpty(CompanyName))
            {
                result.Add(new ValidationResult("Company name is required", new string[] { "CompanyName" }));
            }

            return result;
        }
    }

    public class RegistrationCompleteViewModel
    {
        public string Token { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public bool IsApi { get; set; }
    }

    public class SessionEditModel
    {
        [Required, MaxLength(100), EmailAddressWeb, Placeholder, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required, DataType(DataType.Password), MaxLength(50), Placeholder]
        public string Password { get; set; }

        [StringLength(100)]
        public string DeviceId { get; set; }

        [StringLength(500)]
        public string ReturnUrl { get; set; }
    }

    public class ResetPasswordRequestModel
    {
        [Required, MaxLength(100), EmailAddressWeb, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public Guid Token { get; set; }

        public TimeSpan ExpiryTime { get; set; }

        public bool IsWebApi { get; set; }
    }

    public class ChangePasswordViewModel
    {
        public int UserId { get; set; }

        [Required, DisplayName("Current password"), DataType(DataType.Password), MaxLength(50)]
        public string CurrentPassword { get; set; }

        [Required, DisplayName("New password"), DataType(DataType.Password), MinLength(6), MaxLength(50)]
        public string NewPassword { get; set; }

        public bool Success { get; set; }
    }

    public class UserTokenModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }
    }

    public class ResetPasswordViewModel : ProcessResultsViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Email { get; set; }

        [Required, DisplayName("Token")]
        public string Token { get; set; }

        [Required, DataType(DataType.Password), MinLength(6), MaxLength(50), Placeholder]
        public string NewPassword { get; set; }

        [Required, DataType(DataType.Password), System.ComponentModel.DataAnnotations.Compare("NewPassword"), MaxLength(50), Placeholder]
        public string ConfirmPassword { get; set; }
    }

    public class ProcessResultsViewModel
    {
        public bool Success { get; set; }
    }

    public class OneTimePasswordModel
    {
        public OneTimePasswordModel(Guid token)
        {
            Token = token;
        }

        public Guid Token { get; set; }

        public string Code => (Convert.ToInt32(Token.ToString().Substring(0, 4), 16) % 10000).ToString("D4");
    }

    public class AccountEmailModel
    {
        [Required, EmailAddressWeb, MaxLength(100)]
        public string Email { get; set; }

    }

    public class AccountDetailsEditModel
    {
        [Required, MaxLength(25)]
        public string FirstName { get; set; }

        [Required, MaxLength(25)]
        public string LastName { get; set; }


        [Required, EmailAddressWeb, MaxLength(100), DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        public string ProfilePhotoPath { get; set; }

        [FileMaxSize(8 * 1024 * 1024), FileMinSize]
        [HttpPostedFileExtensions(allowedExtensions: "jpg, jpeg, png, gif")]
        public IFormFile ProfilePhotoFile { get; set; }
    }


    public class NewSessionApiModel
    {
        [Required, MaxLength(100), EmailAddressWeb, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required, DataType(DataType.Password), MaxLength(50)]
        public string Password { get; set; }

        [StringLength(100)]
        public string DeviceId { get; set; }

        public bool Cookieless { get; set; }
    }

    public class LoginCodeRequestApiModel
    {
        [Required, PhoneNumber("AU")]
        public string Phone { get; set; }
    }

    public class LoginCodeResponseApiModel
    {
        public string VerificationToken { get; set; }
    }

    public class SessionSummaryApiModel
    {
        public string UserKey { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public UserStatus Status { get; set; }

        public List<Role> Roles { get; set; }

        public string ProfilePhotoPath { get; set; }

        public DateTime? ExpiresOn { get; set; }

        public ImpersonatorSummaryApiModel Impersonator { get; set; }

    }

    public class ImpersonatorSummaryApiModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public List<UserRoleApiModel> Roles { get; set; }
    }

    public class LoginRoleModel
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public Role Role { get; set; }

        public string RoleDesc
        {
            get
            {
                if (this.Role.IsCompanyRole())
                {
                    String.Format("{0} at {1}", this.Role, this.CompanyName);
                }

                if (this.Role == Role.Administrator)
                    return "Super Admin";

                return Role.GetDescription();
            }
        }

        public int? CompanyId { get; set; }
        public string CompanyName { get; set; }

        public override bool Equals(object obj)
        {
            var cast = obj as LoginRoleModel;
            if (cast == null) return false;

            return this.UserId == cast.UserId && this.Id == cast.Id;
        }
    }


    public class InviteEditModel
    {
        [Required, MaxLength(25)]
        public string FirstName { get; set; }

        [Required, MaxLength(25)]
        public string LastName { get; set; }

        [Required, MaxLength(100), EmailAddressWeb, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [FileMaxSize(8 * 1024 * 1024), FileMinSize]
        [HttpPostedFileExtensions(allowedExtensions: "jpg, jpeg, png, gif")]
        public IFormFile ProfilePhotoFile { get; set; }

        public string ProfilePhotoPath { get; set; }

        public string Token { get; set; }

        public string Inviter { get; set; }

        public InviteRoleViewModel InviteRole { get; set; }
    }

    public class InviteRoleViewModel : IValidatableObject
    {
        [Required]
        public Role? Role { get; set; }

        [EmptyItem, DisplayName("Company")]
        public int? CompanyId { get; set; }
        public SelectList CompanyList { get; set; }
        public string CompanyName { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.Role.Value.IsCompanyRole() && String.IsNullOrEmpty(this.CompanyName) && this.CompanyId == null)
                yield return new ValidationResult("Company name is mandatory", new string[] { "CompanyName" });
        }
    }

    public class InviteUploadModel
    {
        [Required(ErrorMessage = "A csv file must be chosen")]
        [DisplayName("Users CSV file"), HelpText("Bulk upload a list of users")]
        [FileMaxSize(1 * 1024 * 1024), FileMinSize]
        [HttpPostedFileExtensions(allowedExtensions: "csv")]
        public IFormFile InviteFile { get; set; }

    }

    public class InviteUploadItemModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Role Role { get; set; }
        //public string CompanyName { get; set; }
    }

    public class UserImportModel
    {
        [Required]
        public Role? Roles { get; set; }

        [Required]
        public UserStatus? Status { get; set; }

        [EmptyItem("Choose company"), DisplayName("Company")]
        public int? CompanyId { get; set; }

        public SelectList CompanyList { get; set; }

        [Required, HelpText("Csv file of users to import. Fields and header must contain: email, firstname and lastname (in any order or case)")]
        [FileMaxSize(1 * 1024 * 1024), FileMinSize]
        [HttpPostedFileExtensions(allowedExtensions: "csv")]
        public IFormFile CsvFile { get; set; }

    }

    public class UserImportResultModel
    {
        public int Processed { get; set; }

        public int Invited { get; set; }

        public string Path { get; set; }
    }

    public class UserCreateModel
    {
        public UserCreateModel()
        {
            this.UserRoles = new List<RoleSelectionViewModel>();
        }

        [Required, MaxLength(25)]
        public string FirstName { get; set; }

        [Required, MaxLength(25)]
        public string LastName { get; set; }

        [Required, MaxLength(100), EmailAddressWeb, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        public List<RoleSelectionViewModel> UserRoles { get; set; }

        [Required]
        public UserStatus Status { get; set; }

        [MaxLength(50)]
        public string Password { get; set; }

        public IFormFile ProfilePhotoFile { get; set; }

    }
}
