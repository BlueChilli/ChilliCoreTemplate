using AutoMapper;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Models.Stripe;
using ChilliSource.Cloud.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace ChilliCoreTemplate.Service
{
    public class StripeHelperService : BaseService
    {
        private readonly ChilliCoreTemplate.Service.EmailAccount.AccountService _accountService;
        private readonly StripeService _stripe;

        public StripeHelperService(ChilliCoreTemplate.Service.EmailAccount.AccountService accountService, StripeService stripe, IPrincipal user, DataContext context, ProjectSettings config, IFileStorage storage, IWebHostEnvironment environment, IMapper mapper) : base(user, context, config, storage, environment, mapper)
        {
            _accountService = accountService;
            _stripe = stripe;
        }

        internal ServiceResult<Customer> Stripe_Get(string stripeId)
        {
            if (!String.IsNullOrWhiteSpace(stripeId)) return _stripe.Customer_Get(stripeId);
            return Stripe_Get();
        }

        internal ServiceResult<Customer> Stripe_Get()
        {
            if (CompanyId.HasValue)
            {
                var company = Context.Companies.Where(x => x.Id == CompanyId).FirstOrDefault();
                if (company == null) return ServiceResult<Customer>.AsError("Company was not found.");

                if (String.IsNullOrWhiteSpace(company.StripeId))
                {
                    return ServiceResult<Customer>.AsSuccess();
                }
                return _stripe.Customer_Get(company.StripeId);
            }
            var account = Context.Users.FirstOrDefault(x => x.Id == UserId);
            if (account == null) return ServiceResult<Customer>.AsError("Account was not found.");

            if (String.IsNullOrWhiteSpace(account.StripeId))
            {
                return ServiceResult<Customer>.AsSuccess();
            }
            return _stripe.Customer_Get(account.StripeId);
        }

        internal ServiceResult<Customer> Stripe_UpdateOrCreate(string token, string coupon)
        {
            var user = Context.Users.FirstOrDefault(x => x.Id == UserId);
            if (user == null) return ServiceResult<Customer>.AsError("Account was not found.");
            if (CompanyId.HasValue)
            {
                var company = Context.Companies.Where(x => x.Id == CompanyId.Value).FirstOrDefault();
                if (company == null) return ServiceResult<Customer>.AsError("Company was not found.");
                return Stripe_UpdateOrCreate(company, user, token, coupon);
            }
            return Stripe_UpdateOrCreate(user, token, coupon);
        }

        private ServiceResult<Customer> Stripe_UpdateOrCreate(Company company, User user, string token = null, string coupon = null)
        {
            var name = _environment.IsProduction() ? company.Name : $"{_environment.EnvironmentName} - {company.Name}";
            var metadata = new Dictionary<string, string> { { "CompanyId", company.Id.ToString() } };

            if (String.IsNullOrEmpty(company.StripeId))
            {
                var request = _stripe.Customer_Create(
                    new CustomerCreateOptions
                    {
                        Name = name,
                        Email = user.Email,
                        Metadata = metadata,
                        Source = token,
                        Coupon = coupon,
                        Expand = new List<string> { "default_source", "subscriptions" }
                    });
                if (!request.Success) return request;
                company.StripeId = request.Result.Id;
                Context.SaveChanges();
                return ServiceResult<Customer>.AsSuccess(request.Result);
            }
            return _stripe.Customer_Update(company.StripeId,
                new CustomerUpdateOptions
                {
                    Name = name,
                    Email = user.Email,
                    Metadata = metadata,
                    Source = token,
                    Coupon = coupon,
                    Expand = new List<string> { "default_source", "subscriptions" }
                });
        }

        private ServiceResult<Customer> Stripe_UpdateOrCreate(User user, string token = null, string coupon = null, string accountId = null)
        {
            var model = _mapper.Map<StripeCustomerEditModel>(user);
            model.Token = token;
            var customerRequest = _stripe.Customer_AddOrUpdate(model, accountId);
            if (!customerRequest.Success) return ServiceResult<Customer>.CopyFrom(customerRequest);

            if (String.IsNullOrEmpty(user.StripeId))
            {
                user.StripeId = customerRequest.Result.Id;
                Context.SaveChanges();
            }

            return customerRequest;
        }

        public ServiceResult<Customer> Stripe_SetPlan(PaymentPlan planType, Stripe.Customer customer, string firstName, string email)
        {
            var planRequest = Stripe_PlanGet(planType);
            if (!planRequest.Success) return ServiceResult<Customer>.CopyFrom(planRequest);
            var plan = planRequest.Result;

            Subscription subscription = customer.Subscriptions == null ? null : customer.Subscriptions.Where(x => x.IsValid()).FirstOrDefault();

            if (subscription != null && subscription.Items.Any(x => x.Plan.UsageType != plan.UsageType)) //Metered and licenced plans don't mix
            {
                var cancelSubscriptionRequest = _stripe.Subscription_Cancel(subscription.Id);
                if (!cancelSubscriptionRequest.Success) return ServiceResult<Customer>.CopyFrom(cancelSubscriptionRequest);
                subscription = null;
            }

            if (subscription == null)
            {
                var createOptions = new Stripe.SubscriptionCreateOptions { Items = new List<Stripe.SubscriptionItemOptions>(), TrialFromPlan = true };
                createOptions.Items.Add(new Stripe.SubscriptionItemOptions { Plan = plan.Id });
                var createRequest = _stripe.Subscription_Create(customer.Id, createOptions);
                if (!createRequest.Success) return ServiceResult<Customer>.CopyFrom(createRequest);

                _accountService.QueueMail(RazorTemplates.Company_SubscriptionCreated, email, new RazorTemplateDataModel<SubscriptionEmailModel> { Data = new SubscriptionEmailModel { FirstName = firstName, Plan = planType } });
            }
            else
            {
                var updateOptions = new Stripe.SubscriptionUpdateOptions { CancelAtPeriodEnd = false, Items = new List<Stripe.SubscriptionItemOptions>() };
                var item = subscription.Items.First();
                if (item.Plan.Id != plan.Id)
                {
                    updateOptions.Items.Add(new Stripe.SubscriptionItemOptions { Id = item.Id, Plan = plan.Id });
                }
                if (subscription.CancelAtPeriodEnd || updateOptions.Items.Any())
                {
                    var updateSubscriptionRequest = _stripe.Subscription_Update(subscription.Id, updateOptions);
                    if (!updateSubscriptionRequest.Success) return ServiceResult<Customer>.CopyFrom(updateSubscriptionRequest);
                    if (!subscription.CancelAtPeriodEnd)
                        _accountService.QueueMail(RazorTemplates.Company_SubscriptionCreated, email, new RazorTemplateDataModel<SubscriptionEmailModel> { Data = new SubscriptionEmailModel { FirstName = firstName, Plan = planType } });
                }
            }
            return _stripe.Customer_Get(customer.Id);
        }

        private ServiceResult<Stripe.Plan> Stripe_PlanGet(PaymentPlan plan)
        {
            var plansRequest = _stripe.Plan_List();
            if (!plansRequest.Success) return ServiceResult<Stripe.Plan>.CopyFrom(plansRequest);
            var plans = plansRequest.Result;

            var stripePlan = plans
                .Where(x => x.Metadata.ContainsKey("PLAN") && x.Metadata["PLAN"] == plan.ToString().ToUpper())
                .FirstOrDefault();

            if (stripePlan == null) return ServiceResult<Stripe.Plan>.AsError("Plan not found");

            return ServiceResult<Stripe.Plan>.AsSuccess(stripePlan);
        }

        internal void Stripe_CancelSubscriptionForOwner(int id)
        {
            var owner = _accountService.GetAccount(id);
            if (String.IsNullOrEmpty(owner.StripeId)) return;
            var customerRequest = _stripe.Customer_Get(owner.StripeId);
            if (!customerRequest.Success) return;
            var subscription = customerRequest.Result.Subscriptions.Where(x => x.IsValid()).FirstOrDefault();
            if (subscription != null) _stripe.Subscription_Cancel(subscription.Id);
        }

        internal bool Stripe_CustomerHasValidSubscription(string stripeId)
        {
            if (String.IsNullOrEmpty(stripeId)) return false;
            var customerRequest = _stripe.Customer_Get(stripeId);
            if (!customerRequest.Success) return false;

            var customer = customerRequest.Result;
            return customer.Subscriptions.Any(x => x.IsValid());
        }

    }
}
