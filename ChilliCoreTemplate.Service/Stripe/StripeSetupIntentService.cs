using ChilliSource.Cloud.Core;
using Stripe;
using System;
using System.Collections.Generic;

namespace ChilliCoreTemplate.Service
{
    public partial class StripeService
    {

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
