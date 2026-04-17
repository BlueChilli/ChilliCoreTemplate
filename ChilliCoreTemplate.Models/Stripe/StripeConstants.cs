using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Models.Stripe;

public static class StripeConstants
{
    public const string DefaultCurrency = "aud";

    public const string CreditCard = "card";
    public const string DirectDebit = "au_becs_debit";

    public const string SubscriptionTrial = "trialing";
    public const string SubscriptionActive = "active";
    public const string SubscriptionOverdue = "past_due";
    public const string SubscriptionCancelled = "canceled";

    public const string COMPANYID = "CompanyId";
    public const string SYSTEM = "System";
}

public static class StripeErrorCodes
{
    public const string ResourceMissing = "resource_missing";
}