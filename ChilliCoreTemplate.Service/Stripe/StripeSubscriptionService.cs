using ChilliSource.Cloud.Core;
using Microsoft.Extensions.Caching.Memory;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChilliCoreTemplate.Service
{
    public partial class StripeService
    {
        private const string SubscriptionCacheKey = "Subscriptions";

        internal ServiceResult<Subscription> Subscription_Create(string customerId, SubscriptionCreateOptions options)
        {
            options.Customer = customerId;

            var subscriptionService = new SubscriptionService(_client);

            try
            {
                var stripeResult = subscriptionService.Create(options);
                return ServiceResult<Subscription>.AsSuccess(stripeResult);
            }
            catch (Exception ex)
            {
                return ServiceResult<Subscription>.AsError(ex.Message);
            }
        }

        public ServiceResult<Subscription> Subscription_Update(string subscriptionId, SubscriptionUpdateOptions options)
        {
            var subscriptionService = new SubscriptionService(_client);
            try
            {
                if (options.Items != null && !options.Items.Any()) options.Items = null;
                var stripeResult = subscriptionService.Update(subscriptionId, options);
                return ServiceResult<Subscription>.AsSuccess(stripeResult);
            }
            catch (Exception ex)
            {
                return ServiceResult<Subscription>.AsError(ex.Message);
            }
        }

        //public ServiceResult<SubscriptionItem> Subscription_DeleteItem(string subscriptionId, SubscriptionItemDeleteOptions options = null)
        //{
        //    var service = new SubscriptionItemService(_client);
        //    SubscriptionItem stripeResult = null;
        //    try
        //    {
        //        stripeResult = service.Delete(subscriptionId, options ?? new SubscriptionItemDeleteOptions());
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ServiceResult<SubscriptionItem>()
        //        {
        //            Success = false,
        //            Error = ex.Message
        //        };
        //    }
        //    return new ServiceResult<SubscriptionItem>()
        //    {
        //        Result = stripeResult,
        //        Success = true
        //    };
        //}

        public ServiceResult<Subscription> Subscription_Cancel(string subscriptionId, SubscriptionCancelOptions options = null)
        {
            try
            {
                var service = new SubscriptionService(_client);
                var stripeResult = service.Cancel(subscriptionId, options ?? new SubscriptionCancelOptions { });
                return ServiceResult<Subscription>.AsSuccess(stripeResult);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<Subscription>.AsError(ex.Message);
            }
        }

        internal ServiceResult<Subscription> Subscription_Get(string id, SubscriptionGetOptions options = null, bool useCache = true)
        {
            if (String.IsNullOrEmpty(id)) return ServiceResult<Subscription>.AsError("No subscription");

            try
            {
                var service = new SubscriptionService(_client);
                if (options == null && useCache)
                {
                    var key = $"{SubscriptionCacheKey}{id}";
                    if (!_cache.TryGetValue(key, out Subscription subscription))
                    {
                        subscription = service.Get(id, options);
                        _cache.Set(key, subscription, TimeSpan.FromMinutes(60));
                    }
                    return ServiceResult<Subscription>.AsSuccess(subscription);
                }
                else
                {
                    var subscription = service.Get(id, options);
                    return ServiceResult<Subscription>.AsSuccess(subscription);
                }
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<Subscription>.AsError(ex.Message);
            }
        }

        public ServiceResult Subscription_DiscountDelete(string subscriptionId)
        {
            try
            {
                var service = new DiscountService(_client);
                var stripeResult = service.DeleteSubscriptionDiscount(subscriptionId);
                return ServiceResult.AsSuccess();
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult.AsError(ex.Message);
            }
        }

        public ServiceResult<List<Plan>> Plan_List()
        {
            var planService = new PlanService(_client);
            try
            {
                var stripeResult = planService.List(new PlanListOptions { Active = true, Expand = new List<string> { "data.product" } });
                return ServiceResult<List<Plan>>.AsSuccess(stripeResult.Data);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<Plan>>.AsError(ex.Message);
            }
        }

        public ServiceResult<UsageRecord> Plan_Usage(string subscriptionItemId, UsageRecordCreateOptions options)
        {
            var usageService = new UsageRecordService(_client);
            try
            {
                var stripeResult = usageService.Create(subscriptionItemId, options);
                return ServiceResult<UsageRecord>.AsSuccess(stripeResult);
            }
            catch (Exception ex)
            {
                return ServiceResult<UsageRecord>.AsError(ex.Message);
            }
        }
    }
}
