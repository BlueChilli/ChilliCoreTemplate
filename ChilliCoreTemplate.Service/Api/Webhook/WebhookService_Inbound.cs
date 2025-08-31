
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.Distributed;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service.Api
{
    public partial class WebhookService : Service<DataContext>
    {
        private readonly StripeService _stripe;
        private readonly IWebHostEnvironment _env;
        private readonly ProjectSettings _config;
        private readonly IServiceProvider _serviceProvider;

        public WebhookService(BackgroundTaskPrincipal user, DataContext context, AccountService accountService, StripeService stripe, IWebHostEnvironment env,
            IFileStorage fileStorage, ProjectSettings config, Sqids.SqidsEncoder<int> sqids, PdfService pdf, EmailQueueService email, IServiceProvider serviceProvider) : base(user, context)
        {
            _stripe = stripe;
            _env = env;
            _config = config;
            _serviceProvider = serviceProvider;
        }

        public ServiceResult QueueWebhook(WebhookType type, string json)
        {
            var log = new WebhookInbound
            {
                Type = type,
                CreatedOn = DateTime.UtcNow,
                Raw = json
            };
            var saveWebhook = ServiceResult<bool>.AsSuccess(false);
            switch (type)
            {
                case WebhookType.Stripe:
                    saveWebhook = Stripe_LogFromJson(log, json);
                    break;
                case WebhookType.Twilio:
                    saveWebhook = Twilio_LogFromJson(log, json);
                    break;
            }
            if (saveWebhook.Result)
            {
                SaveWebhook(log);
            }
            return ServiceResult.CopyFrom(saveWebhook);
        }

        public async Task ProcessWebhook(ITaskExecutionInfo executionInfo = null)
        {
            try
            {
                using (var scope = ScopeContextFactory.Instance.CreateScope())
                {
                    using (var webhookContext = scope.ServiceProvider.GetService<DataContext>())
                    {
                        var tasks = await webhookContext.WebhooksInbound.Where(t => !t.Processed).Take(20).ToListAsync(); //With the job running every 10 seconds this will allow up to 2 hooks per second to be processed (sequentially).

                        foreach (var task in tasks)
                        {
                            var result = ServiceResult.AsSuccess();
                            if (executionInfo != null)
                            {
                                executionInfo.SendAliveSignal();
                                if (executionInfo.IsCancellationRequested)
                                    break;
                            }

                            try
                            {
                                result = await ProcessWebhook(task, scope.ServiceProvider);
                            }
                            catch (Exception ex)
                            {
                                ex.LogException();
                                result.Success = false;
                                result.Error = ex.Message;
                            }

                            task.Processed = true;
                            task.ProcessedOn = DateTime.UtcNow;
                            task.Success = result.Success;
                            task.Error = String.IsNullOrEmpty(result.Error) ? null : result.Error;
                            await webhookContext.SaveChangesAsync();

                            if (!result.Success && _env.IsProduction())
                            {
                                ErrorLogHelper.LogMessage($"{task.Type} Webhook failed: {result.Error}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.LogException();
            }

        }

        public async Task ProcessWebhook(string webhookId)
        {
            if (String.IsNullOrEmpty(webhookId)) return;
            var webhookIdHash = webhookId.GetIndependentHashCode().Value;
            var task = await Context.WebhooksInbound.Where(l => l.WebhookIdHash == webhookIdHash && l.WebhookId == webhookId).FirstOrDefaultAsync();
            if (task == null) return;

            var result = await ProcessWebhook(task, _serviceProvider);

            task.Processed = true;
            task.ProcessedOn = DateTime.UtcNow;
            task.Success = result.Success;
            task.Error = result.Error;
            await Context.SaveChangesAsync();
        }

        internal async static Task<ServiceResult> ProcessWebhook(WebhookInbound task, IServiceProvider serviceProvider)
        {
            ServiceResult result = ServiceResult.AsSuccess();

            var scope = serviceProvider.CreateScope();
            switch (task.Type)
            {
                case WebhookType.Stripe:
                    result = await Stripe_ProcessWebhook(task, scope);
                    break;
                    //case WebhookType.Twilio:
                    //    result = Twilio_ProcessWebhook(task);
                    //    break;
                    //case WebhookType.Sns:
                    //    result = Sns_ProcessWebhook(task);
                    //    break;
            }
            return result;
        }


        private void SaveWebhook(WebhookInbound model)
        {
            if (String.IsNullOrEmpty(model.WebhookId)) model.WebhookId = Guid.NewGuid().ToString();
            model.WebhookIdHash = model.WebhookId.GetIndependentHashCode().Value;

            var log = Context.WebhooksInbound.Where(l => l.WebhookIdHash == model.WebhookIdHash && l.WebhookId == model.WebhookId && l.Type == model.Type).FirstOrDefault();
            if (log == null)
            {
                Context.WebhooksInbound.Add(model);
                Context.SaveChanges();
            }
            else if (!log.Success && log.Processed)
            {
                log.Processed = false;
                log.ProcessedOn = null;
                log.Raw = model.Raw;
                log.CreatedOn = DateTime.UtcNow;
                log.Error = null;
                Context.SaveChanges();
            }
        }

        public void CreateWebhook(Guid secret)
        {
            if (secret != new Guid("b0cab192-a8d2-4d6c-8cf0-b8607fe35945")) return;

            var baseUrl = _config.BaseUrl;
            var url = (baseUrl.Contains("localhost") ? "https://develop.mysite.com" : baseUrl) + "/api/v1/webhooks/stripe";
            var options = new Stripe.WebhookEndpointCreateOptions
            {
                Url = url,
                ApiVersion = Stripe.StripeConfiguration.ApiVersion,
                EnabledEvents = new List<string>
                {
                    "payment_intent.succeeded"
                },
            };
            _stripe.Webhook_Create(options);
        }

        public async Task CleanWebhooks(ITaskExecutionInfo executionInfo)
        {
            executionInfo.SendAliveSignal();
            if (executionInfo.IsCancellationRequested)
                return;

            //Delete task older than 1 month
            var oneMonth = DateTime.UtcNow.AddMonths(-1);
            var oldTasks = await Context.WebhooksInbound.Where(t => t.CreatedOn < oneMonth).Take(100).ToListAsync();
            Context.WebhooksInbound.RemoveRange(oldTasks);
            await Context.SaveChangesAsync();
        }
    }

}
