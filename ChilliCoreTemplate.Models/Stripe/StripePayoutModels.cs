using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilliCoreTemplate.Models.Stripe
{
    public class StripePayoutDetail
    {
        public StripePayoutDetail()
        {
            Transfers = new List<string>();
        }

        public string Id { get; set; }

        public string UserId { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; }

        public DateTime PaidOn { get; set; }

        public List<string> Transfers { get; set; }
    }
}
