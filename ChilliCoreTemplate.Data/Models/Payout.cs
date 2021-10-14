using ChilliCoreTemplate.Data.EmailAccount;
using System;
using System.ComponentModel.DataAnnotations;

namespace ChilliCoreTemplate.Data
{
    public class Payout
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; }

        [StringLength(50)]
        public string PayoutId { get; set; }

        public decimal Amount { get; set; }

        public DateTime PaidOn { get; set; }

        public DateTime CreatedOn { get; set; }

    }

}
