//using ChilliSource.Cloud.Core;
//using ChilliSource.Cloud.Web.MVC;
//using System;
//using System.Linq;
//using System.Web;
//using System.Web.Security;

//namespace ChilliCoreTemplate.Models.EmailAccount
//{
//    public class AccountRoleProvider : RoleProvider
//    {
//        // Just the bare minimum
//        public AccountRoleProvider() { }
//        public override void AddUsersToRoles(string[] usernames, string[] roleNames) { }
//        public override void CreateRole(string roleName) { }
//        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole) { return false; }
//        public override string[] FindUsersInRole(string roleName, string usernameToMatch) { return new string[0]; }
//        public override string[] GetAllRoles() { return Enum.GetNames(typeof(Role)); }
//        public override string[] GetUsersInRole(string roleName) { return new string[0]; }
//        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames) { }
//        public override bool RoleExists(string roleName) { return false; }
//        public override string ApplicationName { get; set; }

//        // bare minimum - only works for logged in user
//        public override bool IsUserInRole(string username, string roleName)
//        {
//            if (HttpContext.Current.User.Identity.Name != username || !GetAllRoles().Contains(roleName))
//                return false;
//            var authTicket = WebAuthenticationTicket.Current();
//            return authTicket.Roles.IsInRole(roleName);
//        }

//        public override string[] GetRolesForUser(string username)
//        {
//            if (HttpContext.Current.User.Identity.Name != username)
//                return new string[0];
//            var authTicket = WebAuthenticationTicket.Current();
//            return authTicket.Roles.UserRoles();
//        }
//    }
//}