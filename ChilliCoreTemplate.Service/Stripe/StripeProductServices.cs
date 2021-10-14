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

        private const string ProductCacheKey = "Products";

        public ServiceResult<IEnumerable<Product>> Product_List(ProductListOptions options = null, string accountId = null, bool useCache = true)
        {
            try
            {
                var service = new ProductService(_client);
                var defaultOptions = new ProductListOptions { Active = true };
                if (options == null && useCache)
                {
                    var key = $"{ProductCacheKey}{accountId}";
                    if (!_cache.TryGetValue(key, out IEnumerable<Product> products))
                    {
                        products = service.ListAutoPaging(defaultOptions, CreateRequestOptions(accountId));
                        _cache.Set(key, products, TimeSpan.FromMinutes(60));
                    }
                    return ServiceResult<IEnumerable<Product>>.AsSuccess(products);
                }
                else
                {
                    var products = service.ListAutoPaging(options ?? defaultOptions, CreateRequestOptions(accountId));
                    return ServiceResult<IEnumerable<Product>>.AsSuccess(products);
                }
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<IEnumerable<Product>>.AsError(ex.Message);
            }
        }

    }
}
