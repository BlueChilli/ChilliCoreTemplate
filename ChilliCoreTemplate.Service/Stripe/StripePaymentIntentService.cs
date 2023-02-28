using ChilliSource.Cloud.Core;
using Stripe;
using System;

namespace ChilliCoreTemplate.Service
{
    public partial class StripeService
    {

        public ServiceResult<PaymentIntent> PaymentIntent_Create(PaymentIntentCreateOptions options)
        {
            try
            {
                var service = new PaymentIntentService(_client);
                var response = service.Create(options);
                return ServiceResult<PaymentIntent>.AsSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<PaymentIntent>.AsError(ex.Message);
            }
        }

        public ServiceResult<PaymentIntent> PaymentIntent_Get(string id)
        {
            try
            {
                var service = new PaymentIntentService(_client);
                var response = service.Get(id);
                return ServiceResult<PaymentIntent>.AsSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<PaymentIntent>.AsError(ex.Message);
            }
        }
    }
}
