using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Sms;
using ChilliSource.Cloud.Core;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Text;
using Twilio.Clients;

namespace ChilliCoreTemplate.Service.Sms
{
    public class TwilioSmsService : ISmsService
    {
        ProjectSettings _config;
        SmsConfigurationSection _smsConfig;

        public TwilioSmsService(ProjectSettings config)
        {
            _config = config;
            _smsConfig = config.SmsSettings;
        }

        private TwilioRestClient GetClient()
        {
            return new TwilioRestClient(_smsConfig.UserName, _smsConfig.Password);
        }

        public ServiceResult<string> Send(SmsMessageViewModel model)
        {
            if (String.IsNullOrWhiteSpace(model.From))
            {
                model.From = _smsConfig.From;
            }

            var client = GetClient();

            var callback = String.IsNullOrEmpty(_smsConfig.CallbackUrl) ? null : new Uri(_config.ResolveUrl(_smsConfig.CallbackUrl));

            var phoneUtil = PhoneNumberUtil.GetInstance();
            var userPhone = phoneUtil.Parse(model.To, "AU");
            model.To = phoneUtil.Format(userPhone, PhoneNumberFormat.E164);

            var result = Twilio.Rest.Api.V2010.Account.MessageResource.Create(model.To, from: model.From, body: model.Message, client: client, statusCallback: callback);

            if (result == null || !String.IsNullOrEmpty(result.ErrorMessage))
            {
                return ServiceResult<string>.AsError(error: result?.ErrorMessage ?? "Could not send sms due to system error.");
            }


            return ServiceResult<string>.AsSuccess(result.Sid);
        }

        public decimal? Balance()
        {
            return decimal.MaxValue;
        }

    }
}
