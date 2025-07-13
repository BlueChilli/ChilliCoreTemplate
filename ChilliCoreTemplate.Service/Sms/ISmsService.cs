using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Sms;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilliCoreTemplate.Service.Sms
{
    public interface ISmsService
    {
        ServiceResult<string> Send(SmsMessageViewModel message);

        decimal? Balance();
    }

    public class SmsServiceFactory
    {
        readonly ProjectSettings _config;
        readonly EmailQueueService _email;

        public SmsServiceFactory(ProjectSettings config, EmailQueueService email)
        {
            _config = config;
            _email = email;
        }


        public ISmsService CreateService()
        {
            var smsConfig = _config.SmsSettings;
            if (smsConfig == null) throw new ApplicationException("Trying to use FileStorage without setting it up in appsettings");

            switch (smsConfig.Provider)
            {
                case SmsProvider.Email:
                    return new EmailSmsService(_config, _email);
                case SmsProvider.Twilio:
                    return new TwilioSmsService(_config, _email);
                default:
                    throw new ApplicationException($"Unknown Sms Provider: {smsConfig.Provider}");
            }
        }

    }
}
