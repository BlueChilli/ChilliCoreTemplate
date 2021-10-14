using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Core;
using Microsoft.Extensions.Caching.Memory;
using Stripe;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilliCoreTemplate.Service
{
    public partial class StripeService
    {

        private const string PriceCacheKey = "Prices";

        public ServiceResult<IEnumerable<Price>> Price_List(PriceListOptions options = null, string accountId = null, bool useCache = true)
        {
            try
            {
                var service = new PriceService(_client);
                var defaultOptions = new PriceListOptions { Active = true, Expand = new List<string> { "data.product" } };
                if (options == null && useCache)
                {
                    var key = $"{PriceCacheKey}{accountId}";
                    if (!_cache.TryGetValue(key, out IEnumerable<Price> prices))
                    {
                        prices = service.ListAutoPaging(defaultOptions, CreateRequestOptions(accountId));
                        _cache.Set(key, prices, TimeSpan.FromMinutes(60));
                    }
                    return ServiceResult<IEnumerable<Price>>.AsSuccess(prices);
                }
                else
                {
                    var prices = service.ListAutoPaging(options ?? defaultOptions, CreateRequestOptions(accountId));
                    return ServiceResult<IEnumerable<Price>>.AsSuccess(prices);
                }
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<IEnumerable<Price>>.AsError(ex.Message);
            }
        }

        public ServiceResult<Price> Price_Get(string id, string accountId = null)
        {
            try
            {
                var service = new PriceService(_client);
                var response = service.Get(id, new PriceGetOptions { Expand = new List<string> { "product" } }, requestOptions: CreateRequestOptions(accountId));
                return ServiceResult<Price>.AsSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<Price>.AsError(ex.Message);
            }
        }

        public ServiceResult<Price> Price_Update(string id, PriceUpdateOptions options, string accountId = null)
        {
            try
            {
                var service = new PriceService(_client);
                var response = service.Update(id, options, requestOptions: CreateRequestOptions(accountId));
                return ServiceResult<Price>.AsSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<Price>.AsError(ex.Message);
            }
        }

    }
}
