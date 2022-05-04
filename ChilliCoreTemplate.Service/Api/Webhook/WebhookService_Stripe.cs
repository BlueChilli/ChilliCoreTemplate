using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Stripe;
using AutoMapper;
using ChilliSource.Cloud.Core;
using Humanizer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models.EmailAccount;

namespace ChilliCoreTemplate.Service.Api
{
    partial class WebhookService
    {
        private StripeService _stripe;

        private ServiceResult<bool> Stripe_LogFromJson(Webhook_Inbound log, string json)
        {
            var model = Stripe.EventUtility.ParseEvent(json);

            if (model == null)
            {
                log.Error = "Not able to deserialize json";
                log.Processed = true;
                return ServiceResult<bool>.AsError(true, log.Error);
            }
            else
            {
                if (model.Livemode)
                {
                    if (!_env.IsProduction()) return ServiceResult<bool>.AsError("Live message sent to test environment");
                }
                else if (_env.IsProduction())
                {
                    return ServiceResult<bool>.AsError("Test message sent to live environment");
                }

                log.WebhookId = model.Id;
                log.Subtype = model.Type;
            }
            return ServiceResult<bool>.AsSuccess(true);
        }

        private ServiceResult Stripe_ProcessWebhook(Webhook_Inbound task)
        {
            ServiceResult result = ServiceResult.AsSuccess();

            var stripeEvent = Stripe.EventUtility.ParseEvent(task.Raw);

            var userId = stripeEvent.Account;
            var stripeEventResult = _stripe.Event_Get(stripeEvent);
            if (!stripeEventResult.Success)
            {
                result = ServiceResult.CopyFrom(stripeEventResult);
            }
            else
            {
                //stripeEvent = stripeEventResult.Result;
                stripeEvent.Account = userId;

                switch (stripeEvent.Type)
                {
                    case Stripe.Events.ChargeSucceeded:
                        result = ProcessChargeSucceeded(stripeEvent);
                        break;
                    case Stripe.Events.ChargeRefunded:
                        result = ProcessChargeRefunded(stripeEvent);
                        break;
                    case Stripe.Events.ChargeDisputeCreated:
                    case Stripe.Events.ChargeDisputeClosed:
                        result = ProcessChargeDisputed(stripeEvent);
                        break;
                    case Stripe.Events.PayoutPaid:
                        result = ProcessPayoutPaid(stripeEvent);
                        break;
                    case Stripe.Events.TransferCreated:
                    case Stripe.Events.TransferUpdated:
                        result = ProcessTransfer(stripeEvent);
                        break;
                    //case Stripe.Events.TransferFailed:
                    //    result = ProcessTransferFailed(stripeEvent);
                    //    break;
                    case Stripe.Events.InvoiceUpcoming:
                        result = ProcessInvoiceUpcoming(stripeEvent);
                        break;
                    case Stripe.Events.InvoicePaymentSucceeded:
                        result = ProcessInvoicePaymentSucceeded(stripeEvent);
                        break;
                    case Stripe.Events.InvoicePaymentFailed:
                        result = ProcessInvoicePaymentFailed(stripeEvent);
                        break;
                    case Stripe.Events.AccountUpdated:
                        result = Stripe_ProcessAccountUpdated(stripeEvent);
                        break;
                    case Stripe.Events.PaymentIntentSucceeded:
                        result = Stripe_ProcessPaymentIntentSucceeded(stripeEvent);
                        break;
                    case Stripe.Events.CustomerSubscriptionDeleted:
                        result = ProcessSubscriptionDeleted(stripeEvent);
                        break;
                    case Stripe.Events.CustomerDeleted:
                        result = ProcessCustomerDeleted(stripeEvent);
                        break;
                    default:
                        result = ServiceResult.AsError($"Stripe event {stripeEvent.Type} not handled");
                        break;
                }
            }
            return result;
        }

        private ServiceResult ProcessChargeRefunded(Stripe.Event stripeEvent)
        {
            string jsonData = JsonConvert.SerializeObject(stripeEvent.Data.Object);
            var charge = Stripe.Charge.FromJson(jsonData);

            //var transaction = Context.Transactions
            //    .FirstOrDefault(t => t.Stripe.ChargeId == charge.Id);

            //if (transaction == null)
            //    return ServiceResult.AsError($"No transaction matching charge id {charge.Id} was found");


            //transaction.DateRefunded = DateTime.UtcNow;
            //transaction.IsRefunded = true;
            //transaction.Status = "Refunded";

            Context.SaveChanges();

            return ServiceResult.AsSuccess();

        }

        private ServiceResult ProcessChargeDisputed(Stripe.Event stripeEvent)
        {
            string jsonData = JsonConvert.SerializeObject(stripeEvent.Data.Object);
            var dispute = Stripe.Dispute.FromJson(jsonData);

            //var hash = Transaction.CalculateStripe.ChargeIdHashCode(dispute.ChargeId);
            //var transaction = Context.Transactions
            //    .FirstOrDefault(t => t.Stripe.ChargeIdHash == hash && t.Stripe.ChargeId == dispute.ChargeId);

            //if (transaction != null)
            //{
            //    Donation donation = null;
            //    if (transaction.Type == TransactionType.Donation)
            //    {
            //        donation = Context.Donations.Include(d => d.Campaign.Organisation).First(d => d.TransactionId == transaction.Id);
            //    }

            //    //warning_needs_response, warning_under_review, warning_closed, , under_review, charge_refunded, won, or lost
            //    switch (dispute.Status)
            //    {
            //        case "needs_response":
            //            transaction.DateUpdated = DateTime.UtcNow;
            //            transaction.Status = Constants.Transaction_InDispute;

            //            break;
            //        case "lost":
            //            transaction.DateUpdated = DateTime.UtcNow;
            //            transaction.Status = Constants.Transaction_DisputeLost;

            //            break;
            //        case "won":
            //            if (transaction.Status == Constants.Transaction_InDispute)
            //            {
            //                transaction.DateUpdated = DateTime.UtcNow;
            //                transaction.Status = ApiServices.GetTransactionStatus(transaction.TransferId.HasValue, transaction.IsRefunded);
            //            }
            //            break;
            //        default:
            //            return ServiceResult.AsError($"Dispute reason: {dispute.Status} not handled");
            //    }

            //    Context.SaveChanges();
            //    return ServiceResult.AsSuccess();
            //}

            return ServiceResult.AsError($"No transaction matching charge id {dispute.Id} was found");
        }

        private ServiceResult ProcessChargeSucceeded(Stripe.Event stripeEvent)
        {
            string jsonData = JsonConvert.SerializeObject(stripeEvent.Data.Object);
            //var model = Mapper<Stripe.Charge>.MapFromJson(jsonData);

            return ServiceResult.AsSuccess();
        }

        private ServiceResult ProcessPayoutPaid(Stripe.Event stripeEvent)
        {
            var payout = stripeEvent.Data.Object as Stripe.Payout;

            var transactionsRequest = _stripe.BalanceTransaction_List(payout.Id, stripeEvent.Account);
            if (!transactionsRequest.Success) return ServiceResult.CopyFrom(transactionsRequest);
            var transactions = transactionsRequest.Result;

            var model = new StripePayoutDetail
            {
                Id = payout.Id,
                UserId = stripeEvent.Account,
                Amount = payout.Amount / 100.0M,
                Currency = payout.Currency,
                PaidOn = payout.ArrivalDate
            };
            foreach(var transaction in transactions.Where(x => x.Type == "payment"))
            {
                var charge = transaction.Source as Charge;
                if (charge != null)
                {
                    var transferId = charge.SourceTransferId;
                    model.Transfers.Add(transferId);
                }
            }

            //var payoutSaveRequest = _services.Payout_Save(model);
            //if (!payoutSaveRequest.Success) return ServiceResult.CopyFrom(payoutSaveRequest);

            return ServiceResult.AsSuccess();
        }

        private ServiceResult ProcessTransfer(Stripe.Event stripeEvent)
        {
            string jsonData = JsonConvert.SerializeObject(stripeEvent.Data.Object);
            //var model = Mapper<StripeTransfer>.MapFromJson(jsonData);

            return ServiceResult.AsSuccess();
        }

        private ServiceResult ProcessTransferFailed(Stripe.Event stripeEvent)
        {
            string jsonData = JsonConvert.SerializeObject(stripeEvent.Data.Object);
            //var model = Mapper<StripeTransfer>.MapFromJson(jsonData);

            return ServiceResult.AsSuccess();
        }

        private ServiceResult ProcessInvoiceUpcoming(Stripe.Event stripeEvent)
        {
            string jsonData = JsonConvert.SerializeObject(stripeEvent.Data.Object);

            return ServiceResult.AsSuccess();
        }

        private ServiceResult ProcessInvoicePaymentSucceeded(Stripe.Event stripeEvent)
        {
            var model = stripeEvent.Data.Object as Invoice;

            if (model.AmountPaid == 0) return ServiceResult.AsSuccess();

            Company company = Context.Companies.FirstOrDefault(c => c.StripeId == model.CustomerId);
            if (company == null) ServiceResult.AsError($"Company not found for {model.CustomerId}: {stripeEvent.Id}");

            var payment = Context.Payments.FirstOrDefault(x => (x.CompanyId == company.Id) && x.EventId == stripeEvent.Id);
            if (payment != null) return ServiceResult.AsError($"Process invoice payment - payment already recorded: {stripeEvent.Id}");

            if (!String.IsNullOrEmpty(model.ChargeId))
            {
                var chargeRequest = _stripe.Charge_Get(model.ChargeId);
                if (!chargeRequest.Success) return ServiceResult.CopyFrom(chargeRequest);
                model.Charge = chargeRequest.Result;
            }

            var line = model.Lines.FirstOrDefault();

            payment = new Payment
            {
                Amount = model.AmountPaid / 100.0M,
                CompanyId = company.Id,
                PaidOn = DateTime.UtcNow,
                ChargeId = model.ChargeId,
                ReceiptUrl = model.Charge?.ReceiptUrl,
                EventId = stripeEvent.Id,
                Description = line?.Description
            };

            Context.Payments.Add(payment);
            Context.SaveChanges();

            return ServiceResult.AsSuccess();
        }

        private ServiceResult ProcessInvoicePaymentFailed(Stripe.Event stripeEvent)
        {
            var model = stripeEvent.Data.Object as Invoice;
            if (model.AmountDue == 0) return ServiceResult.AsSuccess();

            Company company = Context.Companies.FirstOrDefault(c => c.StripeId == model.CustomerId);
            if (company == null) ServiceResult.AsError($"Company not found for {model.CustomerId}: {stripeEvent.Id}");

            var admin = _accountService.GetCompanyAdmin(company.Id);
            var emailModel = Mapper.Map<AccountViewModel>(admin);

            var bcc = _env.IsProduction() ? new EmailData_Address(_config.AdminEmail) : null;
            _accountService.QueueMail(
                RazorTemplates.Company_PaymentInvoiceFailed,
                admin.Email,
                new RazorTemplateDataModel<AccountViewModel> { Data = emailModel },
                bcc: new List<EmailData_Address> { bcc }
            );

            return ServiceResult.AsSuccess();
        }

        private ServiceResult Stripe_ProcessAccountUpdated(Stripe.Event stripeEvent)
        {
            var account = stripeEvent.Data.Object as Stripe.Account;

            var requirements = account.Requirements;
            if (requirements != null)
            {
                if (!requirements.EventuallyDue.Any() && !requirements.PendingVerification.Any() && account.PayoutsEnabled)
                {
                    //return _services.ManagedAccount_Completed(account.Id);
                }
                else if (!requirements.EventuallyDue.Any())
                {
                    //return _services.ManagedAccount_DetailsProvided(account.Id);
                }
            }

            return ServiceResult.AsSuccess();
        }

        private ServiceResult Stripe_ProcessPaymentIntentSucceeded(Stripe.Event stripeEvent)
        {
            var intent = stripeEvent.Data.Object as Stripe.PaymentIntent;

            if (intent.Metadata.TryGetValue(StripeService.TRANSACTIONID, out var transactionId))
            {
                var guid = new Guid(transactionId);
                //var result = _apiServices.Something_Paid(guid, intent.Charges.First(), intent.CustomerId);
                //return ServiceResult.CopyFrom(result);
            }

            return ServiceResult.AsSuccess();
        }

        private ServiceResult ProcessSubscriptionDeleted(Stripe.Event stripeEvent)
        {
            var model = stripeEvent.Data.Object as Subscription;

            var customerRequest = _stripe.Customer_Get(model.CustomerId);
            if (!customerRequest.Success) return ServiceResult.CopyFrom(customerRequest);
            var customer = customerRequest.Result;

            if (customer.Subscriptions != null && customer.Subscriptions.Any(x => x.IsValid())) return ServiceResult.AsSuccess();

            //Project code goes here

            return ServiceResult.AsSuccess();
        }

        private ServiceResult ProcessCustomerDeleted(Stripe.Event stripeEvent)
        {
            var model = stripeEvent.Data.Object as Customer;

            Company company = Context.Companies.FirstOrDefault(c => c.StripeId == model.Id);

            if (company != null)
            {
                company.StripeId = null;
                company.IsDeleted = true;
                var accounts = Context.Users.Where(x => x.UserRoles.Any(r => r.CompanyId == company.Id && r.Role == Role.CompanyAdmin) && x.Status != UserStatus.Deleted).ToList();
                accounts.ForEach(x => x.Status = UserStatus.Deleted);
            }
            else
            {
                var account = Context.Users.FirstOrDefault(a => a.StripeId == model.Id && a.Status != UserStatus.Deleted);
                if (account != null)
                {
                    account.Status = UserStatus.Deleted;
                    account.StripeId = null;
                }
            }

            Context.SaveChanges();

            return ServiceResult.AsSuccess();
        }

    }
}
