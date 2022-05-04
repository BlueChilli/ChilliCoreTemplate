using ChilliCoreTemplate.Models.Stripe;
using ChilliCoreTemplate.Service;
using ChilliSource.Core.Extensions;
using FoolProof.Core;
using Newtonsoft.Json;
using Stripe;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ChilliCoreTemplate.Models.Api
{
    public class PaymentApiModel
    {
        public int Id { get; set; }

        public decimal Amount { get; set; }

        public string Description { get; set; }

        public string ChargeId { get; set; }

        public DateTime Date { get; set; }

        public string ReceiptUrl { get; set; }

    }

    public class PaymentEditApiModel : IValidatableObject
    {
        public string Token { get; set; }

        public string CouponId { get; set; }

        public PaymentPlan? Plan { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (String.IsNullOrEmpty(Token) && String.IsNullOrEmpty(CouponId) && Plan == null) yield return new ValidationResult("At least one payment configuration option must be selected");
        }
    }

    public class PaymentDetailApiModel
    {
        private string _creditCard { get; set; }

        public string CreditCardText {
            get { return String.IsNullOrEmpty(_creditCard) ? "A credit card has not yet been supplied.." : _creditCard; }
            set { _creditCard = value; }
        }
        public bool CreditCardIsValid { get; set; }

        [ReadOnly(true)]
        public string PromoCode { get; set; }
        public string PromoName { get; set; }
        public decimal Discount { get; set; }

        public PaymentPlan Plan { get; set; }
        public decimal PlanMonthlyBasePrice => Plan.GetData<int>("Price");
        public decimal PlanMonthlySeatPrice => Plan.GetData<int>("SeatPrice");
        public PaymentPlanStatus PlanStatus { get; set; }
        public DateTime? NextPaymentOn { get; set; }
        public decimal? NextPaymentAmount { get; set; }
        public int? SubscriptionDaysRemaining { get; set; }
        public int? FreeTrialDaysRemaining { get; set; }


        [JsonIgnore]
        public string PlanId { get; set; }
        [JsonIgnore]
        public string PlanItemId { get; set; }

        public ApiPagedList<PaymentApiModel> Payments { get; set; }

        public static PaymentDetailApiModel CreateFrom(Customer customer)
        {
            var model = new PaymentDetailApiModel { };

            Subscription subscription = null;
            if (customer != null)
            {
                var coupon = customer.Discount?.Coupon;
                if (coupon != null)
                {
                    model.PromoCode = coupon.Id;
                    model.PromoName = coupon.Name;
                    model.Discount = coupon.PercentOff ?? 0;
                }

                var card = customer.DefaultSource as Card;
                if (card != null)
                {
                    model.CreditCardText = $"Card ending in {card.Last4} that expires on {card.ExpMonth} {card.ExpYear}";
                }
                else if (customer.DefaultSource is Source)
                {
                    var card2 = (customer.DefaultSource as Source)?.Card;
                    if (card2 != null)
                    {
                        model.CreditCardText = $"Card ending in {card2.Last4} that expires on {card2.ExpMonth} {card2.ExpYear}";
                    }
                }
                model.CreditCardIsValid = customer.HasValidCreditCard();

                if (customer.Subscriptions != null) subscription = customer.Subscriptions.Where(x => x.IsValid() && x.Items.Any(i => i.Plan.Metadata.ContainsKey("PLAN"))).FirstOrDefault();
            }

            if (subscription != null)
            {
                var item = subscription.Items.Where(x => x.Plan.Metadata.ContainsKey("PLAN")).FirstOrDefault();
                model.Plan = (PaymentPlan)Enum.Parse(typeof(PaymentPlan), item.Plan.Metadata["PLAN"], ignoreCase: true);
                model.PlanStatus = PaymentPlanStatus.Active;
                model.PlanId = item.Plan.Id;
                model.PlanItemId = item.Id;
                if (subscription.Status == "past_due")
                {
                    model.SubscriptionDaysRemaining = (int)Math.Round((subscription.CurrentPeriodStart.AddDays(28) - DateTime.UtcNow).TotalDays);
                }
                else if (subscription.CancelAtPeriodEnd)
                {
                    var days = (int)Math.Round((subscription.CurrentPeriodEnd - DateTime.UtcNow).TotalDays);
                    model.SubscriptionDaysRemaining = days;
                }
                else
                {
                    model.NextPaymentOn = subscription.CurrentPeriodEnd.ToTimezone().Date;
                    if (subscription.TrialEnd != null) model.FreeTrialDaysRemaining = (subscription.TrialEnd.Value - DateTime.UtcNow).Days;
                }
            }
            else
            {
                model.PlanStatus = PaymentPlanStatus.None;
            }

            return model;
        }

    }

    public class SubscriptionEmailModel
    {
        public string FirstName { get; set; }

        public PaymentPlan Plan { get; set; }

        public int Seats { get; set; }
    }


    public class SubscriptionExpiryNotifyModel
    {
        public int PropertyId { get; set; }

        public string PropertyName { get; set; }

        public int DaysToExpire { get; set; }

        public int DocumentCount { get; set; }

        public int WarrantyCount { get; set; }

        public int MaintenanceCount { get; set; }

        public int ContactCount { get; set; }

        public int PropertyCount { get; set; }

        public string Email { get; set; }

        public PaymentPlan PaymentPlan { get; set; }
    }

    public enum PaymentPlan
    {
        [Data("Price", 49)]
        [Data("SeatPrice", 0)]
        Individual = 1,
        [Data("Price", 99)]
        [Data("SeatPrice", 19)]
        Company
    }

    public enum PaymentPlanStatus
    {
        None,
        Active
    }

    public class SubscriptionDeleteApiModel
    {
        [Required]
        public SubscriptionCancelReason? CancelReason { get; set; }

        [RequiredIf("CancelReason", SubscriptionCancelReason.Other), MaxLength(500)]
        public string OtherCancelReason { get; set; }

    }

    public enum SubscriptionCancelReason
    {
        None = 0,
        Another,
        Temporary,
        Competitor,
        Unnecessary,
        Other
    }
}

