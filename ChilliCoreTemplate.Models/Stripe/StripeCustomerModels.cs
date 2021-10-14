using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilliCoreTemplate.Models.Stripe
{
    public class StripeCustomerEditModel
    {
        public string Id { get; set; }

        public string Email { get; set; }

        public string Description { get; set; }

        public string Token { get; set; }

        public string Card { get; set; }

    }
}
