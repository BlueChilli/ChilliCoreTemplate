using Stripe;
using System;

namespace ChilliCoreTemplate.Models.Stripe
{
    public static class StripeExtensions
    {
        public static string AmountFormatted(this Plan p)
        {
            return (p.Amount.GetValueOrDefault(0) / 100.0).ToString("C");
        }

        public static DateTime? NextPaymentDue(this Subscription s)
        {
            return s.Discount?.End ?? s.CurrentPeriodEnd;
        }

        public static bool IsValid(this Subscription s)
        {
            return s.Status == "trialing" || s.Status == "active" || s.Status == "past_due";
        }

        public static bool HasValidCreditCard(this Customer customer)
        {
            if (customer.DefaultSource is Card)
            {
                var card = customer.DefaultSource as Card;
                return (String.IsNullOrEmpty(card.CvcCheck) || card.CvcCheck == "unavailable" || card.CvcCheck == "pass") && !(card.Deleted ?? false) && new DateTime((int)card.ExpYear, (int)card.ExpMonth, 1).AddMonths(1) > DateTime.UtcNow;
            }
            else if (customer.DefaultSource is Source)
            {
                var card = (customer.DefaultSource as Source)?.Card;
                if (card != null)
                {
                    return (String.IsNullOrEmpty(card.CvcCheck) || card.CvcCheck == "unavailable" || card.CvcCheck == "pass") && new DateTime((int)card.ExpYear, (int)card.ExpMonth, 1).AddMonths(1) > DateTime.UtcNow;
                }
            }
            return false;
        }

        public static bool IsValid(this PaymentMethodCard card)
        {
            var cvcCheck = card.Checks?.CvcCheck;

            return (String.IsNullOrEmpty(cvcCheck) || cvcCheck == "unavailable" || cvcCheck == "pass") && new DateTime((int)card.ExpYear, (int)card.ExpMonth, 1).AddMonths(1) > DateTime.UtcNow;
        }
    }
}
