using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Models.Sms;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliSource.Cloud.Core.Distributed;
using ChilliSource.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service.Sms
{
    public class AsyncDispatchSmsQueue : SmsQueueBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ITemplateViewRenderer _templateViewRenderer;
        private readonly ILogger _logger;
        private readonly ISmsService _sms;

        public AsyncDispatchSmsQueue(
            IServiceProvider serviceProvider,
            ITemplateViewRenderer templateViewRenderer,
            ILoggerFactory loggerFactory,
            ISmsService sms
        )
        {
            _serviceProvider = serviceProvider;
            _templateViewRenderer = templateViewRenderer;
            _logger = loggerFactory?.CreateLogger<AsyncDispatchEmailQueue>();
            _sms = sms;
        }

        public override async Task Enqueue<T>(RazorTemplate template, int userId, string phone, RazorTemplateDataModel<T> model)
        {
            try
            {
                var item = new SmsQueueItem()
                {
                    TemplateId = template.TemplateName,
                    TemplateIdHash = template.TemplateName.GetIndependentHashCode().Value,
                    UserId = userId,
                    QueuedOn = DateTime.UtcNow,
                    IsReady = true
                };

                try
                {
                    var message = await _templateViewRenderer.RenderAsync(template.TemplateName, model);
                    item.Data = new SmsMessageViewModel
                    {
                        Message = message,
                        To = phone                        
                    }.ToJson();
                }
                catch (Exception ex)
                {
                    item.IsReady = false;
                    item.Data = model.ToJson();
                    item.Error = ex.ToString();
                    _logger?.LogError(ex, $"Enqueue");
                }

                await SaveSmsItem(_serviceProvider, item);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Enqueue");
            }
        }

        public override async Task Process(ITaskExecutionInfo executionInfo)
        {
            var sendingWindow = DateTime.UtcNow.AddDays(-2);

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<DataContext>();

                var smsToSend = await context.SmsQueue
                    .Where(x => x.QueuedOn > sendingWindow && x.IsReady && x.SentOn == null)
                    .Where(x => x.RetryCount == null || x.QueuedOn < DateTime.UtcNow.AddSeconds(-120 * (x.RetryCount ?? 0) * (x.RetryCount ?? 0)))
                    .OrderBy(x => x.Error == null ? x.QueuedOn : DateTime.MaxValue)
                    .FirstOrDefaultAsync();

                executionInfo.SendAliveSignal();
                if (executionInfo.IsCancellationRequested || smsToSend == null)
                    return;

                try
                {
                    var result = _sms.Send(smsToSend.Data.FromJson<SmsMessageViewModel>());

                    if (result.Success)
                    {
                        if (!String.IsNullOrEmpty(result.Result))
                        {
                            smsToSend.MessageId = result.Result;
                            smsToSend.MessageIdHash = smsToSend.MessageId.GetIndependentHashCode().Value;
                        }
                        smsToSend.SentOn = DateTime.UtcNow;
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        smsToSend.Error = result.Error;
                        smsToSend.RetryCount = smsToSend.RetryCount.GetValueOrDefault(0) + 1;
                        await context.SaveChangesAsync();
                    }

                }
                catch (Exception ex)
                {
                    smsToSend.Error = ex.Message;
                    smsToSend.RetryCount = smsToSend.RetryCount.GetValueOrDefault(0) + 1;
                    await context.SaveChangesAsync();
                }

                await context.SaveChangesAsync();
            }
        }
    }

}
