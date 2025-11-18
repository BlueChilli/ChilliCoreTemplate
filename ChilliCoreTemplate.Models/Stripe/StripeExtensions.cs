using Humanizer;
using Stripe;
using System;
using System.Collections.Generic;

namespace ChilliCoreTemplate.Models.Stripe;

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
        return s.Status == StripeConstants.SubscriptionTrial || s.Status == StripeConstants.SubscriptionActive || s.Status == StripeConstants.SubscriptionOverdue;
    }

    public static bool HasValidCreditCard(this Customer customer)
    {
        if (customer.DefaultSource is Card)
        {
            var card = customer.DefaultSource as Card;
            return (String.IsNullOrEmpty(card.CvcCheck) || ValidCvcCheckStatus.Contains(card.CvcCheck)) && !(card.Deleted ?? false) && new DateTime((int)card.ExpYear, (int)card.ExpMonth, 1).AddMonths(1) > DateTime.UtcNow;
        }
        else if (customer.DefaultSource is Source)
        {
            var card = (customer.DefaultSource as Source)?.Card;
            if (card != null)
            {
                return (String.IsNullOrEmpty(card.CvcCheck) || ValidCvcCheckStatus.Contains(card.CvcCheck)) && new DateTime((int)card.ExpYear, (int)card.ExpMonth, 1).AddMonths(1) > DateTime.UtcNow;
            }
        }
        return false;
    }

    public static bool IsValid(this PaymentMethod method)
    {
        if (method.Card != null) return method.Card.IsValid();
        if (method.AuBecsDebit != null) return true;

        return false;
    }

    public static string Description(this PaymentMethod method)
    {
        if (method.Card != null) return CardDescription(method.Card.Brand, method.Card.Last4, method.Card.ExpMonth, method.Card.ExpYear);
        if (method.AuBecsDebit != null) return BecsDescription(method.AuBecsDebit.BsbNumber, method.AuBecsDebit.Last4);
        return "Unknown";
    }

    public static string Description(this ChargePaymentMethodDetails method)
    {
        if (method.Card != null) return CardDescription(method.Card.Brand, method.Card.Last4, method.Card.ExpMonth, method.Card.ExpYear);
        if (method.AuBecsDebit != null) return BecsDescription(method.AuBecsDebit.BsbNumber, method.AuBecsDebit.Last4);
        return "Unknown";
    }

    private static string CardDescription(string brand, string last4, long expMonth, long expYear) => $"{brand.Humanize()} ending in {last4}, exp. {expMonth}/{expYear}";

    private static string BecsDescription(string bsb, string last4)
    {
        if (!String.IsNullOrWhiteSpace(bsb))
        {
            var digits = bsb.Replace("-", "").Trim();
            if (digits.Length == 6 && long.TryParse(digits, out _))
            {
                bsb = $"{digits[..3]}-{digits[3..]}";
            }
        }
        return $"BSB {bsb}, account ending in {last4}";
    }

    public static bool IsValid(this PaymentMethodCard card)
    {
        var cvcCheck = card.Checks?.CvcCheck;

        return (String.IsNullOrEmpty(cvcCheck) || ValidCvcCheckStatus.Contains(cvcCheck)) && new DateTime((int)card.ExpYear, (int)card.ExpMonth, 1).AddMonths(1) > DateTime.UtcNow;
    }

    private static List<string> ValidCvcCheckStatus = ["unavailable", "unchecked", "pass"];

    public static bool CheckSystemMetadata(this IHasMetadata o, ProjectSettings config)
    {
        if (o.Metadata.TryGetValue(StripeConstants.SYSTEM, out var baseUrl))
        {
            return baseUrl == config.BaseUrl;
        }
        return false;
    }
}
