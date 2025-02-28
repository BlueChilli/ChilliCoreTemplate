using ChilliSource.Cloud.Core;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChilliCoreTemplate.Service
{
    public partial class StripeService
    {
        public ServiceResult<SetupIntent> SetupIntent_GetOrCreate(string customerId)
        {
            try
            {
                var service = new SetupIntentService(_client);

                var intents = service.List(new SetupIntentListOptions { Customer = customerId, Created = new DateRangeOptions { GreaterThan = DateTime.UtcNow.AddMinutes(-60) } });
                var intent = intents.Where(x => x.Status == "requires_payment_method").FirstOrDefault();

                if (intent != null) return ServiceResult<SetupIntent>.AsSuccess(intent);

                return SetupIntent_Create(new SetupIntentCreateOptions { Customer = customerId });
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<SetupIntent>.AsError(ex.Message);
            }
        }

        public ServiceResult<SetupIntent> SetupIntent_Create(SetupIntentCreateOptions options)
        {
            try
            {
                var service = new SetupIntentService(_client);
                var response = service.Create(options);
                return ServiceResult<SetupIntent>.AsSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<SetupIntent>.AsError(ex.Message);
            }
        }

        public ServiceResult<SetupIntent> SetupIntent_Get(string id)
        {
            try
            {
                var service = new SetupIntentService(_client);
                var response = service.Get(id);
                return ServiceResult<SetupIntent>.AsSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<SetupIntent>.AsError(ex.Message);
            }
        }
    }
}
