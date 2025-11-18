using ChilliCoreTemplate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Data.EmailAccount
{
    [Index(nameof(UserId), nameof(Role), nameof(CompanyId), IsUnique = true)]
    public class UserRole : IValidatableObject
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; }

        public int? CompanyId { get; set; }
        public Company Company { get; set; }

        public List<UserToken> Tokens { get; set; }

        public Role Role { get; set; }

        public RoleStatus? Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.Role.IsCompanyRole() && this.CompanyId == null && this.Company == null)
                yield return new ValidationResult($"Company role - Invalid role '{this.Role}'. Company is missing.", ["Role"]);
        }

        public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
        {
            public void Configure(EntityTypeBuilder<UserRole> builder)
            {
                builder.ToTable(t => t.HasCheckConstraint("CK_UserRoles_Role", "[Role] > 0"));
                builder.ToTable(t => t.HasCheckConstraint("CK_UserRoles_RoleStatus", "[Role] <> 2 or [Status] <> 2"));
            }
        }
    }
}
