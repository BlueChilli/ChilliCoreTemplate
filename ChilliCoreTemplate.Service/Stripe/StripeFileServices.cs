using ChilliSource.Cloud.Core;
using Stripe;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilliCoreTemplate.Service
{
    public partial class StripeService
    {

        public ServiceResult<string> File_Create(FileCreateOptions options, string accountId = null)
        {
            try
            {
                var service = new FileService(_client);
                var result = service.Create(options, CreateRequestOptions(accountId));
                return ServiceResult<string>.AsSuccess(result.Id);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<string>.AsError(error: ex.Message);
            }
        }
    }
}
