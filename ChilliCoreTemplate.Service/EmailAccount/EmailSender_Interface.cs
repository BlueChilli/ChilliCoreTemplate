using ChilliCoreTemplate.Models.EmailAccount;
using ChilliSource.Cloud.Core;
using System;
using System.Net.Mail;
using System.Threading.Tasks;


namespace ChilliCoreTemplate.Service.EmailAccount
{
    /// <summary>
    /// sends an email
    /// </summary>
    public interface IEmailSender
    {
        Task<ServiceResult<string>> SendAsync(EmailData data);
        ServiceResult Send(EmailData data);
    }

    /// <summary>
    ///  email client that sends the email
    /// </summary>
    public interface IEmailClient : IDisposable
    {
        Task<string> SendAsync(MailMessage message);
        void Send(MailMessage message);

    }
}