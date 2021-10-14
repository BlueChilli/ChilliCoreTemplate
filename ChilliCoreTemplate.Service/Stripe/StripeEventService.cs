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
        public ServiceResult<Person> Person_Get(string personId, string accountId, PersonGetOptions options = null)
        {
            try
            {
                var service = new PersonService(_client);
                var response = service.Get(accountId, personId, options: options);
                return ServiceResult<Person>.AsSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<Person>.AsError(ex.Message);
            }
        }

    }
}
