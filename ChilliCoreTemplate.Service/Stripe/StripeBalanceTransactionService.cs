using ChilliSource.Cloud.Core;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service;

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
            int limit = 100;

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

    public async Task<ServiceResult<List<BalanceTransaction>>> BalanceTransaction_ListAsync(string payoutId, string[] types, string accountId = null)
    {
        var result = new List<BalanceTransaction>();
        foreach (var type in types)
        {
            var res = await BalanceTransaction_ListAsync(payoutId, type, accountId);
            if (res.Success)
            {
                result.AddRange(res.Result);
            }
            else
            {
                return ServiceResult<List<BalanceTransaction>>.AsError(res.Error);
            }
        }
        return ServiceResult<List<BalanceTransaction>>.AsSuccess(result);
    }

    public async Task<ServiceResult<List<BalanceTransaction>>> BalanceTransaction_ListAsync(string payoutId, string type = "charge", string accountId = null)
    {
        try
        {
            var result = new List<BalanceTransaction>();
            var service = new BalanceTransactionService(_client);
            int limit = 100;

            await foreach (var item in service.ListAutoPagingAsync(new BalanceTransactionListOptions { Payout = payoutId, Type = type, Limit = limit, Expand = ["data.source"] },
                    CreateRequestOptions(accountId)))
            {
                result.Add(item);
            }

            return ServiceResult<List<BalanceTransaction>>.AsSuccess(result);
        }
        catch (Exception ex)
        {
            if (ex is not StripeException)
            {
                ex.LogException();
            }
            return ServiceResult<List<BalanceTransaction>>.AsError(ex.Message);
        }
    }
}
