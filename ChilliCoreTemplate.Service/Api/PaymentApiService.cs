using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Models.Stripe;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.LinqMapper;
using Microsoft.AspNetCore.Hosting;
using Stripe;
using System;
using System.Linq;
using System.Security.Principal;

namespace ChilliCoreTemplate.Service.Api
{
    public class PaymentApiService : BaseApiService
    {
        private readonly StripeService _stripe;
        private readonly StripeHelperService _stripeHelper;

        public PaymentApiService(StripeService stripe, StripeHelperService stripeHelper, IPrincipal user, DataContext context, ProjectSettings config, IFileStorage fileStorage, IWebHostEnvironment environment) : base(user, context, config, fileStorage, environment)
        {
            _stripe = stripe;
            _stripeHelper = stripeHelper;
        }

        internal static void LinqMapperConfigure(FileStoragePath storagePath)
        {
            LinqMapper.CreateMap<Payment, PaymentApiModel>();
        }

        public IQueryable<Payment> Payment_Authorised()
        {
            return Context.Payments.Where(x => x.CompanyId == CompanyId);
        }

        public ServiceResult<ApiPagedList<PaymentApiModel>> List(ApiPaging paging = null)
        {
            var payments = Payment_Authorised()
                .OrderByDescending(p => p.Id)
                .Materialize<Payment, PaymentApiModel>()
                .ToPagedList(paging ?? new ApiPaging { PageSize = 20 });

            return ServiceResult<ApiPagedList<PaymentApiModel>>.AsSuccess(payments);
        }

        public ServiceResult<PaymentDetailApiModel> Detail(string stripeId = null)
        {
            var customerRequest = _stripeHelper.Stripe_Get(stripeId);
            if (!customerRequest.Success) return ServiceResult<PaymentDetailApiModel>.CopyFrom(customerRequest);
            var customer = customerRequest.Result;

            var model = PaymentDetailApiModel.CreateFrom(customer);

            model.Payments = List().Result;
            if (model.NextPaymentOn.HasValue)
            {
                var upcomingInvoiceRequest = _stripe.Invoice_Upcoming(customer.Id);
                if (!upcomingInvoiceRequest.Success) return ServiceResult<PaymentDetailApiModel>.CopyFrom(upcomingInvoiceRequest);
                model.NextPaymentAmount = (upcomingInvoiceRequest.Result.AmountRemaining / 100M);
            }

            return ServiceResult<PaymentDetailApiModel>.AsSuccess(model);
        }

        public ServiceResult<PaymentDetailApiModel> Edit(PaymentEditApiModel model)
        {
            ServiceResult<Coupon> couponRequest = null;
            var couponId = model.CouponId?.Trim().ToUpper();

            // Validate coupon if provided
            if (!String.IsNullOrWhiteSpace(couponId))
            {
                couponRequest = _stripe.Coupon_Get(couponId);
                if(!couponRequest.Success || !couponRequest.Result.Valid || couponRequest.Result.PercentOff == null)
                {
                    return ServiceResult<PaymentDetailApiModel>.AsError($"Coupon code '{couponId}' does not exist or is not valid anymore.");
                }
            }

            var token = String.IsNullOrWhiteSpace(model.Token) ? null : model.Token;
            couponId = String.IsNullOrWhiteSpace(couponId) ? null : couponRequest?.Success == true ? couponRequest?.Result?.Id : null;

            var updateRequest = _stripeHelper.Stripe_UpdateOrCreate(token, couponId);
            if (!updateRequest.Success) return ServiceResult<PaymentDetailApiModel>.CopyFrom(updateRequest);
            var customer = updateRequest.Result;

            var details = PaymentDetailApiModel.CreateFrom(customer);
            if (model.Plan == null && !String.IsNullOrWhiteSpace(model.Token) && details.PlanStatus == PaymentPlanStatus.None)
            {
                return ServiceResult<PaymentDetailApiModel>.AsError("Plan is required if customer does not have a subscription and a token is passed in");
            }
            
            if (model.Plan.HasValue)
            {
                if (!details.CreditCardIsValid) return ServiceResult<PaymentDetailApiModel>.AsError("A valid credit credit must be entered to subscribe to a plan");
                var planRequest = _stripeHelper.Stripe_SetPlan(model.Plan.Value, customer, User.UserData().FirstName, User.UserData().Email);
                if (!planRequest.Success) return ServiceResult<PaymentDetailApiModel>.CopyFrom(planRequest);
            }

            return Detail(customer.Id);
        }

        public ServiceResult<PaymentDetailApiModel> Delete()
        {
            var customerRequest = _stripeHelper.Stripe_Get();
            if (!customerRequest.Success) return ServiceResult<PaymentDetailApiModel>.CopyFrom(customerRequest);
            var customer = customerRequest.Result;

            if (customer != null && customer.Subscriptions != null)
            {
                var subscriptionToCancel = customer.Subscriptions.Where(x => x.IsValid()).FirstOrDefault();
                if (subscriptionToCancel != null && !subscriptionToCancel.CancelAtPeriodEnd)
                {
                    var detail = PaymentDetailApiModel.CreateFrom(customer);
                    if (detail.PlanStatus == PaymentPlanStatus.Active)
                    {
                        var cancelRequest = _stripe.Subscription_Update(subscriptionToCancel.Id, new Stripe.SubscriptionUpdateOptions { CancelAtPeriodEnd = true });
                        if (!cancelRequest.Success) return ServiceResult<PaymentDetailApiModel>.CopyFrom(cancelRequest);
                    }
                }
            }

            return Detail(customer?.Id);
        }

    }
}
