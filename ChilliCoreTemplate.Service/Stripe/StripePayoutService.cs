using ChilliCoreTemplate.Models.Stripe;
using ChilliSource.Cloud.Core;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service
{
    public partial class StripeService
    {
        public async Task<ServiceResult<Payout>> Payout_CreateAsync(string accountId, decimal amount)
        {
            try
            {
                var service = new PayoutService(_client);
                var options = new PayoutCreateOptions
                {
                    Amount = (long)(amount * 100),
                    Currency = StripeConstants.DefaultCurrency
                };
                var result = await service.CreateAsync(options, CreateRequestOptions(accountId));
                return ServiceResult<Payout>.AsSuccess(result);
            }
            catch (Exception ex)
            {
                if (ex is not StripeException)
                {
                    ex.LogException();
                }
                return ServiceResult<Payout>.AsError(ex.Message);
            }
        }

    }
}
