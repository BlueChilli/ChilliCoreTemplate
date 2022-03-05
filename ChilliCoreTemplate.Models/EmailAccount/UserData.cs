using AutoMapper;
using ChilliCoreTemplate.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChilliCoreTemplate.Models.EmailAccount
{
    public class UserData
    {
        private UserRoleModel[] _currentRoles = new UserRoleModel[0];

        public UserData() { }

        public UserData(IEnumerable<UserRoleModel> roles)
        {
            this.SetCurrentRoles(roles);
        }

        public void SetCurrentRoles(IEnumerable<UserRoleModel> roles)
        {
            this._currentRoles = roles.Select(r => (UserRoleModel)r.Clone()).ToArray();
        }

        public UserRoleModel[] CurrentRoles { get { return _currentRoles; } set { this.SetCurrentRoles(value); } }

        public List<Role> Roles => this._currentRoles.Select(r => r.Role).ToList();

        public int UserId { get; set; }

        public UserStatus Status { get; set; }

        public string Name => $"{FirstName} {LastName}".Trim();

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string ProfilePhotoPath { get; set; }

        public string Timezone { get; set; }

        public int? CompanyId { get; set; }

        public string CompanyLogoPath { get; set; }

        public string CompanyName { get; set; }

        public UserData Impersonator { get; set; }
        public int? UserDeviceId { get; set; }

        internal string IdentityName()
        {
            return String.IsNullOrEmpty(Name) ? "Unknown" : Name;
        }

        public UserData Clone()
        {
            var clone = (UserData)this.MemberwiseClone();
            clone.SetCurrentRoles(this.CurrentRoles);

            clone.Impersonator = this.Impersonator != null ? this.Impersonator.Clone() : null;
            return clone;
        }

        public UserData GetImpersonator()
        {
            if (Impersonator != null) return Impersonator;
            return this;
        }

        public bool IsImpersonated()
        {
            return (Impersonator != null && Impersonator.UserId != this.UserId);
        }

        public void ImpersonatedBy(UserData currentUser)
        {
            if (currentUser != null)
                this.Impersonator = currentUser.Clone();
        }

        public void RemoveImpersonation()
        {
            var impersonator = this.Impersonator;
            if (impersonator == null)
                return;
            Mapper.Map<UserData, UserData>(impersonator.Clone(), this);
        }

        public bool CanImpersonate(AccountViewModel target)
        {
            return target.Status != UserStatus.Deleted && this.CurrentRoles.Any(currentRole => target.UserRoles.Any(targetRole => currentRole.CanImpersonate(targetRole)));
        }

        private IEnumerable<UserRoleApiModel> GetCurrentRolesApiModel()
        {
            return this.CurrentRoles.Select(r => new UserRoleApiModel() { Role = r.Role, CompanyId = r.CompanyId });
        }

        public ImpersonatorSummaryApiModel ImpersonatorSummary(UserData impersonator)
        {
            return new ImpersonatorSummaryApiModel
            {
                FirstName = impersonator.FirstName,
                LastName = impersonator.LastName,
                Roles = impersonator.GetCurrentRolesApiModel().ToList(),
                Email = impersonator.Email
            };
        }

        public List<int> ImpersonationChain()
        {
            var chain = new List<int>();
            var user = this;
            while (user.IsImpersonated())
            {
                chain.Insert(0, user.Impersonator.UserId);
                user = user.Impersonator;
            }
            return chain;
        }

        public bool IsInRole(Role role)
        {
            return this.CurrentRoles.Any(r => r.Role == role);
        }

        public bool IsInRole(UserRoleModel accountRoleModel)
        {
            return this.CurrentRoles.Any(r => r.Equals(accountRoleModel));
        }

        public IEnumerable<int?> GetCompanyIds()
        {
            return this.CurrentRoles.Where(r => r.Role.IsCompanyRole() && r.CompanyId != null)
                       .Select(r => r.CompanyId);
        }
    }

    public class UserRoleModel : ICloneable
    {
        public Role Role { get; set; }

        public int? CompanyId { get; set; }

        public int? MasterCompanyId { get; set; }

        public string CompanyName { get; set; }

        public bool CanImpersonate(UserRoleModel other)
        {
            if (this.Role.IsCompanyRole())
            {
                return this.CompanyId == other.CompanyId && this.Role == Role.CompanyAdmin;
            }

            return this.Role == Role.Administrator && other.Role != Role.Administrator;
        }

        public override bool Equals(object obj)
        {
            var cast = (obj as UserRoleModel);
            if (cast == null)
                return false;

            return this.Role == cast.Role && this.CompanyId == cast.CompanyId;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}

