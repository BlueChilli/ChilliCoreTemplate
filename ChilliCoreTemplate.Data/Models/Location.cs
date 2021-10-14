using ChilliCoreTemplate.Data.EmailAccount;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ChilliCoreTemplate.Data
{
    public class Location
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; }

        public List<LocationUser> Users { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [MaxLength(50)]
        public string Timezone { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

    }

    public class LocationUser
    {
        public int Id { get; set; }

        public int LocationId { get; set; }
        public Location Location { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public DateTime CreatedOn { get; set; }

    }
}
