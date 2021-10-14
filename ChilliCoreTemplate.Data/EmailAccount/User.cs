using ChilliSource.Cloud.Core;
using ChilliCoreTemplate.Models;
using ChilliSource.Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using System.Linq;

namespace ChilliCoreTemplate.Data.EmailAccount
{
    // This is the EF model
    public partial class User
    {
        public int Id { get; set; }

        public virtual List<UserSession> Sessions { get; set; }

        public virtual List<UserToken> Tokens { get; set; }

        public virtual List<UserRole> UserRoles { get; set; }

        public virtual List<UserDevice> Devices { get; set; }

        public virtual List<UserActivity> Activities { get; set; }

        [StringLength(100)]
        public string Email { get { return _Email; } set { _Email = value; EmailHash = CommonLibrary.CalculateHash(value); } }
        private string _Email;

        public int? EmailHash { get; set; }

        [StringLength(25)]
        public string FirstName { get { return _FirstName; } set { _FirstName = value?.Trim(); FullName = String.Concat(_FirstName, " ", _LastName).Trim(); } }
        private string _FirstName;

        [StringLength(25)]
        public string LastName { get { return _LastName; } set { _LastName = value?.Trim(); FullName = String.Concat(_FirstName, " ", _LastName).Trim(); } }
        private string _LastName;

        [StringLength(55)]
        public string FullName { get; set; }

        #region Phone
        [StringLength(20)]
        public string Phone { get; set; }

        public int PhoneHash { get { return GetPhoneHash(this.Phone); } set { } }

        public static string GetPhoneDigits(string value)
        {
            if (value == null)
                return null;

            var regexObj = new Regex(@"[^\d]");
            return regexObj.Replace(value, "");
        }

        public static int GetPhoneHash(string value)
        {
            return GetPhoneDigits(value).GetIndependentHashCode() ?? 0;
        }
        #endregion


        [MaxLength(100)]
        public string ProfilePhotoPath { get; set; }

        public UserStatus Status { get; set; }

        [StringLength(256)]
        public string PasswordHash { get; set; }

        public Guid PasswordSalt { get; set; }

        public bool PasswordAutoGenerated { get; set; }

        public DateTime? LastLoginDate { get; set; }

        public DateTime? LastRetryDate { get; set; }

        public int NumOfRetries { get; set; }

        public int LoginCount { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }

        public DateTime? ActivatedDate { get; set; }

        public DateTime? ClosedDate { get; set; }

        public DateTime? LastPasswordChangedDate { get; set; }

        public DateTime? InvitedDate { get; set; }

        public bool IsTooManyRetries
        {
            get
            {
                return LastRetryDate.HasValue && LastRetryDate.Value.AddMinutes(10) >= DateTime.UtcNow && NumOfRetries >= 3;
            }
        }

        public UserRole GetLatestUserRole()
        {
            return this.UserRoles.OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id).FirstOrDefault();
        }

        public string GetToken(UserTokenType type) => Tokens.FirstOrNew(t => t.Type == type).Token.ToShortGuid().ToString();

        public bool ConfirmPassword(string password) => PasswordHash == password.SaltedHash(PasswordSalt.ToString());        

        public bool HasCompany()
        {
            return GetFirstCompany() != null;
        }

        public Company GetFirstCompany()
        {
            if (this.UserRoles == null) return null;
            return this.UserRoles.Where(r => r.CompanyId != null).Select(r => r.Company).FirstOrDefault();
        }

        public int? GetFirstCompanyId()
        {
            return GetFirstCompany()?.Id;
        }

        public bool HasRole(Role role, int? companyId = null)
        {
            return this.UserRoles.Any(r => r.Role.HasFlag(role) && (companyId == null || (r.CompanyId == companyId && !r.Company.IsDeleted)));
        }

    }
}
