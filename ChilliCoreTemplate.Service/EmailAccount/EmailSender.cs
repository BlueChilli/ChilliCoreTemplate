using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Core.Extensions;
using Dasync.Collections;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;


namespace ChilliCoreTemplate.Service.EmailAccount
{
    /// <summary>
    /// sends an email
    /// </summary>
    public interface IEmailSender
    {
        Task<ServiceResult> SendAsync(EmailData data);
        ServiceResult Send(EmailData data);
    }

    /// <summary>
    ///  email client that sends the email
    /// </summary>
    public interface IEmailClient : IDisposable
    {
        bool EnableSsl { get; set; }

        ICredentialsByHost Credentials { get; set; }

        Task SendAsync(MailMessage message);
        void Send(MailMessage message);

    }

    /// <summary>
    /// email client that sends the email
    /// </summary>
    public class EmailClient : IEmailClient
    {

        private readonly SmtpClient _client;

        public EmailClient(string host, int port)
        {
            _client = new SmtpClient(host, port);
        }

        public EmailClient(string host, int port, string username, string password, bool enableSsl = false)
        {
            _client = new SmtpClient(host, port)
            {
                Credentials = !String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(password) ? new NetworkCredential(username, password) : null,
                EnableSsl = enableSsl
            };
        }

        public void Dispose()
        {
            _client?.Dispose();
        }

        public bool EnableSsl { get => _client.EnableSsl; set => _client.EnableSsl = value; }
        public ICredentialsByHost Credentials
        {
            get => _client.Credentials;
            set => _client.Credentials = value;
        }

        public Task SendAsync(MailMessage message)
        {
            return _client.SendMailAsync(message);
        }

        public void Send(MailMessage message)
        {
            _client.Send(message);
        }
    }

    /// <summary>
    /// sends email
    /// </summary>
    public class EmailSender : IEmailSender
    {
        private readonly ProjectSettings _settings;
        private readonly IFileStorage _storage;
        private readonly Func<MailConfigurationSection, IEmailClient> _emailClientFactory;
        private readonly ILogger _logger;

        public EmailSender(
            ProjectSettings settings,
            IFileStorage storage,
            Func<MailConfigurationSection, IEmailClient> emailClientFactory,
            ILogger logger)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings)); ;
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _emailClientFactory = emailClientFactory ?? throw new ArgumentNullException(nameof(emailClientFactory)); ;
            _logger = logger;
        }

        /// <summary>
        /// sends email asynchronously
        /// </summary>
        /// <param name="data">object that consits of email information such as to, from address</param>
        /// <returns>ServiceResult</returns>
        public async Task<ServiceResult> SendAsync(EmailData data)
        {
            var mailSettings = _settings.MailSettings;

            var client = _emailClientFactory.Invoke(mailSettings);

            try
            {
                var attachments = new ConcurrentBag<Attachment>();
                await data.Attachments.ParallelForEachAsync(
                     async attachment =>
                     {
                         var at = await attachment.LoadAsync(_storage);
                         if (at != null)
                         {
                             attachments.Add(new Attachment(at.Stream, attachment.FileName, attachment.MimeType));
                         }
                     },
                     5);

                var message = CreateMessage(data, mailSettings);

                // add attachments
                Array.ForEach(attachments.ToArray(), a => message.Attachments.Add(a));

                await client.SendAsync(message).ConfigureAwait(false);

                return ServiceResult.AsSuccess();

            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Sending email failed with {data}");
                return ServiceResult.AsError(ex.ToString());
            }
            finally
            {
                client.Dispose();
            }

        }

        /// <summary>
        /// sends email synchronously
        /// </summary>
        /// <param name="data">object that consits of email information such as to , from address</param>
        /// <returns>ServiceResult</returns>
        public ServiceResult Send(EmailData data)
        {

            var mailSettings = _settings.MailSettings;

            var client = _emailClientFactory.Invoke(mailSettings);


            try
            {
                var attachments = new List<Attachment>();

                foreach (var f in data.Attachments)
                {
                    var attachment = f.Load(_storage, null);
                    attachments.Add(new Attachment(attachment.Stream, f.FileName, attachment.MimeType));
                }

                var message = CreateMessage(data, mailSettings);

                // add attachments
                attachments.ForEach(a => message.Attachments.Add(a));

                client.Send(message);

                return ServiceResult.AsSuccess();

            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Sending email failed with {data}");
                return ServiceResult.AsError(ex.ToString());
            }
            finally
            {
                client.Dispose();
            }
        }

        private static MailMessage CreateMessage(EmailData data, MailConfigurationSection mailSettings)
        {
            var message = new MailMessage();
            var htmlMessage =
                AlternateView.CreateAlternateViewFromString(data.MessageHtml, null, MediaTypeNames.Text.Html);
            var plainMessage =
                AlternateView.CreateAlternateViewFromString(data.MessageText, null, MediaTypeNames.Text.Plain);


            var from = data.From ?? mailSettings.From;

            // Basic message properties            
            message.From = @from.ToMailAddress();

            if (data.ReplyTo != null)
            {
                message.ReplyToList.Add(data.ReplyTo.ToMailAddress());
            }

            if (mailSettings.RedirectTo != null)
            {
                message.To.Add(mailSettings.RedirectTo.ToMailAddress());
            }
            else
            {
                message.To.Add(data.To.DefaultTo(message.From.Address));
            }


            if (mailSettings.Bcc != null)
            {
                message.Bcc.Add(mailSettings.Bcc.ToMailAddress());
            }

            if (data.Bcc != null)
            {
                foreach (var bcc in data.Bcc.Where(x => x != null)) message.Bcc.Add(bcc.ToMailAddress());
            }

            message.Subject = data.Subject;

            // Add the alternate views to the message
            message.AlternateViews.Add(plainMessage);
            message.AlternateViews.Add(htmlMessage);
            return message;
        }
    }
}