using ChilliSource.Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Models
{
    public enum UserStatus
    {
        Registered = 1,
        Activated,
        Deleted,
        Anonymous
    }

    public enum Role
    {
        [Order(1)]
        Administrator = 1,
        [Order(4)]
        User = 2,
        [Order(2)]
        CompanyAdmin = 4,
        [Order(3)]
        CompanyUser = 8
    }

    public enum RoleStatus
    {
        Invited = 1,
        Applied
    }

    public static class RoleHelper
    {
        public static bool IsCompanyRole(this Role? role)
        {
            if (role == null) return false;
            return IsCompanyRole(role.Value);
        }

        public static bool IsCompanyRole(this Role role)
        {
            return role == Role.CompanyAdmin || role == Role.CompanyUser;
        }
    }

    public enum Gender
    {
        Female = 0,
        Male = 1
    }

    public enum UserTokenType
    {
        Password = 1,
        Invite,
        Activate,
        Login,
        OneTimePassword
    }

    public static class AccountCommon
    {
        public const string System = "System";
        public const string User = "User";
        public const string Administrator = "Administrator";
        public const string CompanyAdmin = "CompanyAdmin";
        public const string CompanyUser = "CompanyUser";
    }

    public enum OnboardingStep
    {
        SetupCompany = 1,
        Complete
    }

    public enum UserConfirmationMethod
    {
        Link,   //Sent upon account creation (welcome email) and subsequent logins if not activated
        OneTimePassword //Sent upon account creation and subsequent logins if not activated. Welcome email sent upon activation.
    }
}
