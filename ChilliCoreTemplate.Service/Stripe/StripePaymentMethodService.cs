using ChilliSource.Cloud.Core;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service
{
    public partial class StripeService
    {
        public ServiceResult<PaymentMethod> PaymentMethod_Get(string id)
        {
            try
            {
                var service = new Stripe.PaymentMethodService(_client);
                var response = service.Get(id);
                return ServiceResult<PaymentMethod>.AsSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<PaymentMethod>.AsError(ex.Message);
            }
        }

        public ServiceResult<PaymentMethod>PaymentMethod_Delete(string id)
        {
            try
            {
                var service = new Stripe.PaymentMethodService(_client);
                var response = service.Detach(id);
                return ServiceResult<PaymentMethod>.AsSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<PaymentMethod>.AsError(ex.Message);
            }
        }

    }
}
