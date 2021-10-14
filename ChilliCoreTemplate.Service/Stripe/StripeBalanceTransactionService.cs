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
        public ServiceResult<List<BalanceTransaction>> BalanceTransaction_List(string payoutId, string accountId = null)
        {
            try
            {
                var result = new List<BalanceTransaction>();

                var service = new BalanceTransactionService(_client);

                bool isMore = true;
                string lastId = null;
                int limit = 20;

                while (isMore)
                {
                    var detailsForTransfer = service.List(
                        new BalanceTransactionListOptions { Payout = payoutId, StartingAfter = lastId, Limit = limit, Expand = new List<string> { "data.source" } },
                        CreateRequestOptions(accountId));

                    isMore = detailsForTransfer.Count() == limit;
                    if (detailsForTransfer.Count() > 0)
                    {
                        result.AddRange(detailsForTransfer.Data);
                        lastId = detailsForTransfer.Last().Id;
                    }
                }

                return ServiceResult<List<BalanceTransaction>>.AsSuccess(result);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<List<BalanceTransaction>>.AsError(ex.Message);
            }
        }

    }
}
