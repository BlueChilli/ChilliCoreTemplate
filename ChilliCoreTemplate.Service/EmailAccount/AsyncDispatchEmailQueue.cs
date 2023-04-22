using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.Distributed;
using ChilliSource.Core.Extensions;
using Dasync.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ChilliCoreTemplate.Service.EmailAccount
{
    public class AsyncDispatchEmailQueue : EmailQueueBase
    {
        private readonly IFileStorage _fileStorage;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITemplateViewRenderer _templateViewRenderer;
        private readonly ILogger _logger;

        public AsyncDispatchEmailQueue(
            IFileStorage fileStorage,
            IServiceProvider serviceProvider,
            ITemplateViewRenderer templateViewRenderer,
            ILoggerFactory loggerFactory
        )
        {
            _fileStorage = fileStorage;
            _serviceProvider = serviceProvider;
            _templateViewRenderer = templateViewRenderer;
            _logger = loggerFactory?.CreateLogger<AsyncDispatchEmailQueue>();
        }

        public override Task Enqueue(EmailQueueItem<IEmailTemplateDataModel> queuedItem)
        {
            return Enqueue<IEmailTemplateDataModel>(queuedItem);
        }

        public override async Task Enqueue<T>(EmailQueueItem<T> queuedItem)
        {
            try
            {

                var record = new Email(queuedItem.Template)
                {
                    UserId = queuedItem.UserId,
                    TrackingId = Guid.NewGuid(),
                    Recipient = queuedItem.Email,
                    DateQueued = DateTime.UtcNow
                };

                var templateDataModel = queuedItem.Model as IEmailTemplateDataModel;
                if (templateDataModel != null)
                {
                    templateDataModel.TrackingId = record.TrackingId;
                }

                try
                {
                    var message =
                        await _templateViewRenderer.RenderAsync(queuedItem.Template.TemplateName, queuedItem.Model);

                    var subject = templateDataModel?.Subject
                            .DefaultTo(queuedItem.Subject.DefaultTo(queuedItem.Template.Subject));

                    var attachments = new ConcurrentBag<IEmailAttachment>();

                    await queuedItem.Attachments.ParallelForEachAsync(
                        async attachment =>
                        {
                            var file = await attachment.SaveAsync("Emails/Data", record.TrackingId, attachment.FileName, _fileStorage);
                            attachments.Add(file);
                        }, 5);

                    var f = attachments.ToList();

                    var emailData = new EmailData.Builder()
                        .From(queuedItem.From)
                        .To(queuedItem.Email)
                        .Subject(subject)
                        .Attachments(f)
                        .ReplyTo(queuedItem.ReplyTo)
                        .Bcc(queuedItem.Bcc)
                        .Html(message)
                        .Build();

                    var byteArrayContent = emailData.ToByteArray();
                    var modelPath = await _fileStorage.SaveAsync(
                        new StorageCommand { FileName = $"{record.TrackingId}.dat", Folder = "Emails/Data" }
                            .SetByteArraySource(byteArrayContent));

                    record.Model = modelPath;
                    record.IsReady = true;
                }
                catch (Exception ex)
                {
                    record.Model = queuedItem.ToJson();
                    record.Error = ex.ToString();
                    _logger?.LogError(ex, $"Enqueue");
                }

                await SaveEmailRecord(_serviceProvider, record);

            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Enqueue");
            }

        }

        public override Task<IEmailWorkTask> Dequeue()
        {
            var r = EmailWorkTask.CreateTask(async (executionInfo, provider) =>
            {
                var sendingWindow = DateTime.UtcNow.AddDays(-2);
                var sendRate = 5; //Sends maximum of 5 messages per second. This will depend on your acccounts send rate limit.

                List<Email> emailsToSend;
                using (var scope = provider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetService<DataContext>();

                    emailsToSend = await context.Emails
                        .AsNoTracking()
                        .Where(e => e.DateQueued > sendingWindow && e.IsReady && !e.IsSent && !e.IsSending)
                        .Where(e => e.RetryCount == null || e.DateQueued < DateTime.UtcNow.AddSeconds(-120 * (e.RetryCount ?? 0) * (e.RetryCount ?? 0)))
                        .OrderBy(e => e.Error == null ? e.DateQueued : DateTime.MaxValue)
                        .Take(sendRate * 10) // Runs for 10 seconds max
                        .ToListAsync();

                    executionInfo.SendAliveSignal();
                    if (executionInfo.IsCancellationRequested || emailsToSend.Count == 0)
                        return;

                    emailsToSend.ForEach(x => x.IsSending = true);
                    await context.SaveChangesAsync();
                }

                var throttle = new TaskThrottler(sendRate);

                var tasks = emailsToSend.AsParallel()
                            .Select(email => throttle.Enqueue(() => SendEmailAsync(executionInfo, provider, email)))
                            .ToArray();

                await Task.WhenAll(tasks);
            }, _serviceProvider);

            return Task.FromResult(r);
        }

        private async static Task SendEmailAsync(ITaskExecutionInfo executionInfo, IServiceProvider provider, Email email)
        {
            executionInfo.SendAliveSignal();

            using (var scope = provider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<DataContext>();
                var sender = scope.ServiceProvider.GetService<IEmailSender>();
                var storage = scope.ServiceProvider.GetService<IFileStorage>();

                try
                {
                    var response = await storage.GetContentAsync(email.Model, null);
                    var model = response.Stream.DeserializeTo<EmailData>();
                    model.To = email.Recipient; //So email can be adjusted via database editing
                    var result = await sender.SendAsync(model);

                    context.Emails.Attach(email);
                    if (result.Success)
                    {
                        email.DateSent = DateTime.UtcNow;
                        email.IsSent = true;
                        email.IsSending = false;
                        email.Error = null;
                        await context.SaveChangesAsync();

                    }
                    else
                    {
                        email.IsSending = false;
                        email.Error = result.Error;
                        email.RetryCount = email.RetryCount.GetValueOrDefault(0) + 1;
                        await context.SaveChangesAsync();
                    }

                }
                catch (Exception ex)
                {
                    context.Emails.Attach(email);
                    email.IsSending = false;
                    email.Error = ex.Message;
                    email.RetryCount = email.RetryCount.GetValueOrDefault(0) + 1;
                    await context.SaveChangesAsync();
                }
            }
        }

    }
}