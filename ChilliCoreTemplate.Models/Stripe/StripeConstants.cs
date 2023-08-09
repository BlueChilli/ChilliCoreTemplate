using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Models.Stripe
{
    //For use with paymentmethods api
    public static class StripeConstants
    {
        public const string CreditCard = "card";
        public const string DirectDebit = "au_becs_debit";
    }
}
