using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Core.Extensions;
using Dasync.Collections;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;


namespace ChilliCoreTemplate.Service.EmailAccount
{

    /// <summary>
    /// email client that sends the email
    /// </summary>
    public class EmailClient : IEmailClient
    {

        private readonly string _host;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;

        public EmailClient(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public EmailClient(string host, int port, string username, string password, bool enableSsl = false) //TLS is determined by port number
        {
            _host = host;
            _port = port;
            _username = username;
            _password = password;
        }

        public void Dispose()
        {
        }

        public async Task<string> SendAsync(MailMessage message)
        {
            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                client.Connect(_host, _port);
                if (!String.IsNullOrEmpty(_username) && !String.IsNullOrEmpty(_password))
                    client.Authenticate(_username, _password);
                var result = await client.SendAsync((MimeMessage)message);
                client.Disconnect(true);
                return result;
            }
        }

        public void Send(MailMessage message)
        {
            throw new NotImplementedException();
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
        public async Task<ServiceResult<string>> SendAsync(EmailData data)
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

                var result = await client.SendAsync(message);

                if (!result.StartsWith("Ok", StringComparison.OrdinalIgnoreCase)) return ServiceResult<string>.AsError(error: result);
                result = result.Substring(2).Trim();
                return ServiceResult<string>.AsSuccess(String.IsNullOrEmpty(result) ? null : result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Sending email failed with {data}");
                return ServiceResult<string>.AsError(error: ex.ToString());
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
            throw new NotImplementedException();
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

            var to = data.To.DefaultTo(message.From.Address);
            foreach(var recipient in to.Split(';')) 
            {
                if (String.IsNullOrEmpty(recipient)) continue;
                if (mailSettings.Quarantine.ShouldQuarantine(recipient))
                {
                    message.To.Add(mailSettings.Quarantine.Quarantine(recipient));
                }
                else
                {
                    message.To.Add(recipient);
                }
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