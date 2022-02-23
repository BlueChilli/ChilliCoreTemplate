using ChilliCoreTemplate.Models;
using Microsoft.Extensions.Caching.Memory;
using Stripe;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilliCoreTemplate.Service
{
    public partial class StripeService : IService
    {

        private readonly ProjectSettings _config;
        private readonly StripeClient _client;
        private readonly IMemoryCache _cache;

        internal const string TRANSACTIONID = "TransactionId";

        public StripeService(ProjectSettings config, IMemoryCache memoryCache)
        {
            _config = config;
            if (!String.IsNullOrEmpty(config.StripeSettings.SecretApiKey)) _client = new StripeClient(config.StripeSettings.SecretApiKey);
            _cache = memoryCache;
        }

        private RequestOptions CreateRequestOptions(string accountId)
        {
            return accountId == null ? null : new RequestOptions { StripeAccount = accountId };
        }


    }
}
