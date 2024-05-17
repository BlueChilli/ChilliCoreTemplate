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
        public async Task<ServiceResult<Event>> Event_GetAsync(Event model, EventGetOptions options = null)
        {
            try
            {
                var service = new EventService(_client);
                var response = await service.GetAsync(model.Id, options: options, requestOptions: CreateRequestOptions(model.Account));
                return ServiceResult<Event>.AsSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<Event>.AsError(ex.Message);
            }
        }
    }
}
