using ChilliSource.Cloud.Web.MVC;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ChilliCoreTemplate.Models.Admin
{
    public class ChangeAccountRoleModel
    {
        [Required]
        public int Id { get; set; }

        [Required, DisplayName("Role")]
        public List<Role> Roles { get; set; }

        public SelectList RoleList { get; set; }
    }

    public class ChangeUserStatusModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public UserStatus Status { get; set; }

        public SelectList StatusList { get; set; } = EnumHelper.GetValues<UserStatus>().Where(x => x != UserStatus.Anonymous).ToSelectList(v => v, t => t.GetDescription());
    }

}
