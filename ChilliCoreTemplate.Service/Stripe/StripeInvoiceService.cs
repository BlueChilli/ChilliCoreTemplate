using ChilliSource.Cloud.Core;
using Stripe;
using System;

namespace ChilliCoreTemplate.Service
{
    public partial class StripeService
    {

        public ServiceResult<Invoice> Invoice_Create(InvoiceCreateOptions options)
        {
            try
            {
                var service = new InvoiceService(_client);
                var response = service.Create(options);
                return ServiceResult<Invoice>.AsSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<Invoice>.AsError(ex.Message);
            }
        }

        public ServiceResult<Invoice> Invoice_Pay(string invoiceId, InvoicePayOptions options = null)
        {
            try
            {
                var service = new InvoiceService(_client);
                var response = service.Pay(invoiceId, options);
                return ServiceResult<Invoice>.AsSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<Invoice>.AsError(ex.Message);
            }
        }

        public ServiceResult<Invoice> Invoice_Finalize(string invoiceId, InvoiceFinalizeOptions options = null)
        {
            try
            {
                var service = new InvoiceService(_client);
                var response = service.FinalizeInvoice(invoiceId, options);
                return ServiceResult<Invoice>.AsSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<Invoice>.AsError(ex.Message);
            }
        }

        public ServiceResult<InvoiceItem> InvoiceLineItem_Create(InvoiceItemCreateOptions options, string accountId = null)
        {
            try
            {
                var service = new InvoiceItemService(_client);
                var response = service.Create(options, requestOptions: CreateRequestOptions(accountId));
                return ServiceResult<InvoiceItem>.AsSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<InvoiceItem>.AsError(ex.Message);
            }
        }

        public ServiceResult<InvoiceItem> InvoiceLineItem_Delete(string invoiceItemId, string accountId = null)
        {
            try
            {
                var service = new InvoiceItemService(_client);
                var response = service.Delete(invoiceItemId, requestOptions: CreateRequestOptions(accountId));
                return ServiceResult<InvoiceItem>.AsSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<InvoiceItem>.AsError(ex.Message);
            }
        }

        public ServiceResult<Invoice> Invoice_Upcoming(string customerId, string accountId = null)
        {
            try
            {
                var service = new InvoiceService(_client);
                var response = service.Upcoming(new UpcomingInvoiceOptions { Customer = customerId }, requestOptions: CreateRequestOptions(accountId));
                return ServiceResult<Invoice>.AsSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<Invoice>.AsError(ex.Message);
            }
        }

    }
}
