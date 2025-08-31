using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Core;
using Stripe;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilliCoreTemplate.Service
{
    public partial class StripeService
    {

        public ServiceResult<Charge> Charge_Create(ChargeCreateOptions request, string accountId = null)
        {
            try
            {
                var service = new ChargeService(_client);
                var response = service.Create(request, CreateRequestOptions(accountId));
                return ServiceResult<Charge>.AsSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<Charge>.AsError(ex.Message);
            }
        }

        public ServiceResult<Charge> Charge_Get(string id, ChargeGetOptions options = null, string accountId = null)
        {
            try
            {
                var service = new ChargeService(_client);
                var response = service.Get(id, options, CreateRequestOptions(accountId));
                return ServiceResult<Charge>.AsSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<Charge>.AsError(ex.Message);
            }
        }

        public ServiceResult<Charge> Charge_Update(string id, ChargeUpdateOptions options, string accountId = null)
        {
            try
            {
                var service = new ChargeService(_client);
                var response = service.Update(id, options, requestOptions: CreateRequestOptions(accountId));
                return ServiceResult<Charge>.AsSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<Charge>.AsError(ex.Message);
            }
        }

        public ServiceResult<Refund> Charge_Refund(string chargeId, long? amountInCents = null)
        {
            try
            {
                var service = new RefundService(_client);
                var response = service.Create(new RefundCreateOptions { Charge = chargeId, Amount = amountInCents, Metadata = new() { { StripeService.SYSTEM, _config.BaseUrl } } });
                return ServiceResult<Refund>.AsSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<Refund>.AsError(ex.Message);
            }
        }


    }
}
