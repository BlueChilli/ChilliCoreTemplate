
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
using System.Security.Principal;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service.Api
{
    public partial class WebhookService : Service<DataContext>
    {
        private readonly IWebHostEnvironment _env;
        private readonly IFileStorage _fileStorage;
        private readonly ProjectSettings _config;
        private readonly AccountService _accountService;

        public WebhookService(BackgroundTaskPrincipal user, DataContext context, AccountService accountService, StripeService stripe, IWebHostEnvironment env, IFileStorage fileStorage, ProjectSettings config) : base(user, context)
        {
            _accountService = accountService;
            _stripe = stripe;
            _env = env;
            _fileStorage = fileStorage;
            _config = config;
        }

        public ServiceResult QueueWebhook(WebhookType type, string json)
        {
            var log = new Webhook_Inbound
            {
                Type = type,
                Timestamp = DateTime.UtcNow,
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

        public async Task ProcessWebhook(ITaskExecutionInfo executionInfo)
        {
            try
            {
                using (var scope = ScopeContextFactory.Instance.CreateScope())
                {
                    using (var webhookContext = scope.ServiceProvider.GetService<DataContext>())
                    {
                        var tasks = await webhookContext.Webhooks_Inbound.Where(t => !t.Processed).Take(20).ToListAsync(); //With the job running every 10 seconds this will allow up to 2 hooks per second to be processed (sequentially).

                        foreach (var task in tasks)
                        {
                            var result = ServiceResult.AsSuccess();
                            executionInfo.SendAliveSignal();
                            if (executionInfo.IsCancellationRequested)
                                break;

                            try
                            {
                                result = ProcessWebhook(task);
                            }
                            catch (Exception ex)
                            {
                                ex.LogException();
                                result.Success = false;
                                result.Error = ex.Message;
                            }

                            task.Processed = true;
                            task.Success = result.Success;
                            task.Error = String.IsNullOrEmpty(result.Error) ? null : result.Error;
                            webhookContext.SaveChanges();

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

        public void ProcessWebhook(string webhookId)
        {
            if (String.IsNullOrEmpty(webhookId)) return;
            var webhookIdHash = webhookId.GetIndependentHashCode().Value;
            var task = Context.Webhooks_Inbound.Where(l => l.WebhookIdHash == webhookIdHash && l.WebhookId == webhookId).FirstOrDefault();
            if (task == null) return;

            var result = ProcessWebhook(task);

            task.Processed = true;
            task.Success = result.Success;
            task.Error = result.Error;
            Context.SaveChanges();
        }

        private ServiceResult ProcessWebhook(Webhook_Inbound task)
        {
            ServiceResult result = ServiceResult.AsSuccess();

            switch (task.Type)
            {
                case WebhookType.Stripe:
                    result = Stripe_ProcessWebhook(task);
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


        private void SaveWebhook(Webhook_Inbound model)
        {
            if (String.IsNullOrEmpty(model.WebhookId)) model.WebhookId = Guid.NewGuid().ToString();
            model.WebhookIdHash = model.WebhookId.GetIndependentHashCode().Value;

            var log = Context.Webhooks_Inbound.Where(l => l.WebhookIdHash == model.WebhookIdHash && l.WebhookId == model.WebhookId && l.Type == model.Type).FirstOrDefault();
            if (log == null)
            {
                Context.Webhooks_Inbound.Add(model);
                Context.SaveChanges();
            }
            else if (!log.Success && log.Processed)
            {
                log.Processed = false;
                log.Raw = model.Raw;
                log.Timestamp = DateTime.UtcNow;
                log.Error = null;
                Context.SaveChanges();
            }
        }

        public async Task CleanWebhooks(ITaskExecutionInfo executionInfo)
        {
            executionInfo.SendAliveSignal();
            if (executionInfo.IsCancellationRequested)
                return;

            //Delete task older than 1 month
            var oneMonth = DateTime.UtcNow.AddMonths(-1);
            var oldTasks = await Context.Webhooks_Inbound.Where(t => t.Timestamp < oneMonth).Take(100).ToListAsync();
            Context.Webhooks_Inbound.RemoveRange(oldTasks);
            await Context.SaveChangesAsync();
        }
    }

}
