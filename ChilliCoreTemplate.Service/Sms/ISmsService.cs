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
        readonly AccountService _accountService;

        public SmsServiceFactory(ProjectSettings config, AccountService accountService)
        {
            _config = config;
            _accountService = accountService;
        }


        public ISmsService CreateService()
        {
            var smsConfig = _config.SmsSettings;
            if (smsConfig == null) throw new ApplicationException("Trying to use FileStorage without setting it up in appsettings");

            switch (smsConfig.Provider)
            {
                case SmsProvider.Email:
                    return new EmailSmsService(_config, _accountService);
                case SmsProvider.Twilio:
                    return new TwilioSmsService(_config, _accountService);
                default:
                    throw new ApplicationException($"Unknown Sms Provider: {smsConfig.Provider}");
            }
        }

    }
}
