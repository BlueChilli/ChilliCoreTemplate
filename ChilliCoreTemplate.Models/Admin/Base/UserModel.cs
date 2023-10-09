using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web.MVC;
using ChilliCoreTemplate.Models.EmailAccount;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Http; using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using ChilliSource.Core.Extensions;

namespace ChilliCoreTemplate.Models.Admin
{
    public class UserListModel
    {
        [EmptyItem]
        public int? CompanyId { get; set; }
        public SelectList CompanyList { get; set; }

        [EmptyItem("Any role")]
        public Role? Role { get; set; }
        public SelectList RoleList { get; set; } = EnumHelper.ToList<Role>().ToSelectList();

        [EmptyItem("Any status")]
        public UserStatus? Status { get; set; }

        [Placeholder("Search"), MaxLength(100)]
        public string Search { get; set; }

        public List<StatisticModel> Statistics { get; set; }
    }

    public class InviteManageModel : InviteEditModel
    {
        public List<AccountViewModel> Pending { get; set; }

        public List<SelectListItem> RoleSelectionOptions { get; set; }
    }

    public class UserSummaryViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get { return String.Concat(FirstName, " ", LastName).Trim(); } }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DataLinkModel Company { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }

        [JsonIgnore]
        public List<UserRoleModel> UserRoles { get; set; }
        public string LastLoginOn { get; set; }
    }

    public class UsersExportModel
    {
        public List<Role> Roles { get; set; } = new List<Role>();
        public SelectList RoleList { get; set; }
    }

    public class UserExportModel
    {
        public int Id { get; set; }

        public string Company { get; set; }

        public string MasterCompany { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Role { get; set; }

        public string Status { get; set; }

        public DateTime? LastLoginOn { get; set; }
    }
}