using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ChilliCoreTemplate.Models.Admin
{
    public class ChangeAccountRoleModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public Role? Role { get; set; }

        public SelectList RoleList { get; set; }

        [DisplayName("Company")]
        public int? CompanyId { get; set; }
    }

    public class ChangeUserStatusModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public UserStatus Status { get; set; }
    }

}
