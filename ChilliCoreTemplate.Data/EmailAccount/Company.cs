using ChilliCoreTemplate.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Data.EmailAccount
{
    public class Company
    {
        public virtual List<UserRole> UserRoles { get; set; }

        public int Id { get; set; }

        public int? MasterCompanyId { get; set; }
        public Company MasterCompany { get; set; }

        public Guid Guid { get; set; }

        [Required]
        public Guid? ApiKey { get; set; }

        [MaxLength(50)]
        public string StripeId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        public string LogoPath { get; set; }

        [StringLength(100)]
        public string Website { get; set; }

        [MaxLength(1000)]
        public string Notes { get; set; }

        [Required, StringLength(50)]
        public string Timezone { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedById { get; set; }
        public User DeletedBy { get; set; }
        public bool IsSetup { get; set; }

        public static Company CreateNew(string name = "Company")
        {
            return new Company()
            {
                Name = name,
                Guid = Guid.NewGuid(),
                ApiKey = Guid.NewGuid(),
                Timezone = CommonLibrary.DefaultTimezone,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }

}
