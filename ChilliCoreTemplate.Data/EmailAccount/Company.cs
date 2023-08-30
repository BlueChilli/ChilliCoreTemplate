using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Core.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Data.EmailAccount
{
    public class Company : IExternalId
    {
        public int Id { get; set; }

        public int? MasterCompanyId { get; set; }
        public Company MasterCompany { get; set; }

        public virtual List<UserRole> UserRoles { get; set; }

        public List<Company> SubCompanies { get; set; }

        public Guid Guid { get; set; }

        [MaxLength(50)]
        public string ExternalId { get { return _ExternalId; } set { _ExternalId = value; ExternalIdHash = CommonLibrary.CalculateHash(value); } }
        private string _ExternalId;
        public int? ExternalIdHash { get; set; }

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

        #region Address
        public bool IsManualAddress { get; set; }

        [MaxLength(100)]
        public string Address { get; set; }

        [StringLength(50)]
        public string Street { get; set; }

        [StringLength(50)]
        public string Suburb { get; set; }

        [StringLength(50)]
        public string State { get; set; }

        [StringLength(10)]
        public string Postcode { get; set; }

        [StringLength(50)]
        public string Country { get; set; }

        [StringLength(2)]
        public string Region { get; set; }

        [Required, StringLength(50)]
        public string Timezone { get; set; }
        #endregion

        [DateTimeKind]
        public DateTime CreatedAt { get; set; }

        [DateTimeKind]
        public DateTime UpdatedAt { get; set; }

        [DateTimeKind]
        public DateTime? DeletedAt { get; set; }
        public int? DeletedById { get; set; }
        public User DeletedBy { get; set; }
        public bool IsDeleted { get; set; }

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
