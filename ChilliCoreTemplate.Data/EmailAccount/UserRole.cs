using ChilliCoreTemplate.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Data.EmailAccount
{
    public class UserRole : IValidatableObject
    {
        public int Id { get; set; }
        
        public DateTime CreatedAt { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public Role Role { get; set; }

        public int? CompanyId { get; set; }
        public Company Company { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.Role.IsCompanyRole() && this.CompanyId == null && this.Company == null)
                yield return new ValidationResult($"Company role - Invalid role '{this.Role.ToString()}'. Company is missing.", new string[] { "Role" });
        }
    }
}
