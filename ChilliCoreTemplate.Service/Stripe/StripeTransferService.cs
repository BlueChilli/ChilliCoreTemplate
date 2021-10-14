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
        public ServiceResult<Transfer> Transfer_Create(TransferCreateOptions options)
        {
            try
            {
                var service = new TransferService(_client);
                var response = service.Create(options);
                return ServiceResult<Transfer>.AsSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<Transfer>.AsError(ex.Message);
            }
        }

        public ServiceResult<Transfer> Transfer_Get(string transferId)
        {
            try
            {
                var service = new TransferService(_client);
                var response = service.Get(transferId);
                return ServiceResult<Transfer>.AsSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<Transfer>.AsError(ex.Message);
            }
        }

    }
}
