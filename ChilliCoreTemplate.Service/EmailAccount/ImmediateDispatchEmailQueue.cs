using System;
using System.Threading.Tasks;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliSource.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace ChilliCoreTemplate.Service.EmailAccount
{
    public class ImmediateDispatchEmailQueue : EmailQueueBase
    {
        private readonly IServiceProvider _provider;
        private readonly ITemplateViewRenderer _templateViewRenderer;
        private readonly IEmailSender _emailSender;

        public ImmediateDispatchEmailQueue(
            IServiceProvider provider,
            ITemplateViewRenderer templateViewRenderer,
            IEmailSender emailSender)
        {
            _provider = provider;
            _templateViewRenderer = templateViewRenderer;
            _emailSender = emailSender;

        }

        public override Task Enqueue(EmailQueueItem<IEmailTemplateDataModel> queuedItem)
        {
            return Enqueue<IEmailTemplateDataModel>(queuedItem);
        }

        public override async Task Enqueue<T>(EmailQueueItem<T> queuedItem)
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

            var message =
                await _templateViewRenderer.RenderAsync(queuedItem.Template.TemplateName, queuedItem.Model);

            var subject = templateDataModel?.Subject
                            .DefaultTo(queuedItem.Subject.DefaultTo(queuedItem.Template.Subject));

            var emailData = new EmailData.Builder()
                .From(queuedItem.From)
                .To(queuedItem.Email)
                .Subject(subject)
                .Attachments(queuedItem.Attachments)
                .ReplyTo(queuedItem.ReplyTo)
                .Bcc(queuedItem.Bcc)
                .Html(message)
                .Build();

            var result = await _emailSender.SendAsync(emailData);
            if (result.Success)
            {
                record.IsSent = true;
                record.IsReady = true;
                record.DateSent = DateTime.UtcNow;
            }
            else
            {
                record.Error = result.Error;
            }

            await SaveEmailRecord(_provider, record);

            if (!result.Success)
            {
                throw new ApplicationException(result.Error);
            }
        }

        public override Task<IEmailWorkTask> Dequeue()
        {
            return new ValueTask<IEmailWorkTask>(EmailWorkTask.EmptyTask(_provider)).AsTask();
        }
    }
}