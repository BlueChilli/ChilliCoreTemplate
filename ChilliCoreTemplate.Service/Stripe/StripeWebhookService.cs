using ChilliSource.Cloud.Core;
using Stripe;
using System;

namespace ChilliCoreTemplate.Service
{
    public partial class StripeService
    {
        public ServiceResult<WebhookEndpoint> Webhook_Create(WebhookEndpointCreateOptions options)
        {
            try
            {
                var service = new WebhookEndpointService(_client);
                var response = service.Create(options);
                return ServiceResult<WebhookEndpoint>.AsSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<WebhookEndpoint>.AsError(ex.Message);
            }
        }

    }
}
