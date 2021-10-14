using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Models.Sms;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Text;
using Twilio.Clients;

namespace ChilliCoreTemplate.Service.Sms
{
    public class EmailSmsService : ISmsService
    {
        ProjectSettings _config;
        AccountService _accountService;

        public EmailSmsService(ProjectSettings config, AccountService accountService)
        {
            _config = config;
            _accountService = accountService;
        }

        public ServiceResult<string> Send(SmsMessageViewModel model)
        {
            
            if (String.IsNullOrEmpty(model.To) || !new EmailAddressWebAttribute().IsValid(model.To))
            {
                //If email is not supplied reroute to default email address
                model.To = $"{_config.ProjectName.ToLower()}@mailinator.com";
            }

            _accountService.QueueMail(RazorTemplates.SendSmsViaEmail, model.To, new RazorTemplateDataModel<string> { Data = model.Message.Replace("\n", "<br/>") });

            return ServiceResult<string>.AsSuccess();
        }

        public decimal? Balance()
        {
            return decimal.MaxValue;
        }

    }
}
