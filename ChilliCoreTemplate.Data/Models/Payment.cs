using ChilliCoreTemplate.Data.EmailAccount;
using System;
using System.ComponentModel.DataAnnotations;

namespace ChilliCoreTemplate.Data
{
    public class Payment
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; }

        public decimal Amount { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        [MaxLength(200)]
        public string ChargeId { get; set; }

        public DateTime PaidOn { get; set; }

        [MaxLength(50)]
        public string EventId { get; set; }

        [MaxLength(200)]
        public string ReceiptUrl { get; set; }

    }
}
