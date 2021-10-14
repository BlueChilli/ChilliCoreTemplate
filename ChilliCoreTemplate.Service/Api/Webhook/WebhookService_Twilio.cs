using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Models.Sms;
using ChilliSource.Cloud.Core;
using ChilliSource.Core.Extensions;
using Humanizer;
using System;
using System.Linq;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace ChilliCoreTemplate.Service.Api
{
    partial class WebhookService
    {

        private ServiceResult<bool> Twilio_LogFromJson(Webhook_Inbound log, string json)
        {
            TwilioSmsBaseModel model = null;
            try
            {
                model = json.FromJson<TwilioSmsBaseModel>();
            }
            catch
            {

            }

            if (model == null)
            {
                log.Error = "Not able to deserialize json";
                log.Processed = true;
                return ServiceResult<bool>.AsError(true, log.Error);
            }
            else
            {
                //log.WebhookId = Default to new guid
            }
            return ServiceResult<bool>.AsSuccess(true);
        }

        private ServiceResult Twilio_ProcessWebhook(Webhook_Inbound task)
        {
            var model = task.Raw.FromJson<TwilioSmsBaseModel>();
            if (model.Type == TwilioSmsType.Status)
                return Status(task);
            //else
            //    return Message(task);
            return ServiceResult.AsError("Task type not handled");
        }

        //private ServiceResult Message(Webhook_Inbound task)
        //{
        //    var model = task.Raw.FromJson<TwilioSmsMessageModel>();
        //    var hash = model.SmsSid.GetIndependentHashCode();

        //    var util = PhoneNumbers.PhoneNumberUtil.GetInstance();
        //    var number = util.Parse(model.From, "AU");
        //    var formattedNumber = util.Format(number, PhoneNumbers.PhoneNumberFormat.NATIONAL).Replace(" ", "");

        //    var phoneHash = Data.EmailAccount.User.GetPhoneHash(formattedNumber);
        //    var users = Context.Users
        //        .Where(x => x.PhoneHash == phoneHash && x.Phone == formattedNumber && x.Status != UserStatus.Deleted)
        //        .Where(x => x.UserRoles.Any(r => r.Company.MessagesAreMarketing))
        //        .ToList();
        //    if (!users.Any()) return ServiceResult.AsError("Twilio - user not found for: {0}".FormatWith(formattedNumber));

        //    if (model.Command == TwilioMessageCommand.Stop)
        //    {
        //        users.ForEach(x => x.SmsOptOut = true);
        //    }
        //    else if (model.Command == TwilioMessageCommand.Start)
        //    {
        //        users.ForEach(x => x.SmsOptOut = false);
        //    }
        //    Context.SaveChanges();

        //    return ServiceResult.AsSuccess();
        //}

        private ServiceResult Status(Webhook_Inbound task)
        {
            var model = task.Raw.FromJson<TwilioSmsStatusModel>();
            var hash = model.SmsSid.GetIndependentHashCode();

            var message = Context.SmsQueue.FirstOrDefault(x => x.MessageIdHash == hash && x.MessageId == model.SmsSid);
            if (message == null) return ServiceResult.AsError("Twilio - message not found for: {0}".FormatWith(model.SmsSid));

            switch (model.SmsStatus)
            {
                case TwilioSmsStatus.Failed:
                case TwilioSmsStatus.Undelivered:
                    message.Error = GetMessage(model.SmsSid).ErrorMessage;
                    break;
                case TwilioSmsStatus.Sent:
                    break;
                case TwilioSmsStatus.Delivered:
                    message.DeliveredOn = DateTime.UtcNow;
                    break;
                default:
                    return ServiceResult.AsError("Twilio - status not handled: {0}".FormatWith(model.SmsStatus));
            }

            Context.SaveChanges();

            return ServiceResult.AsSuccess();
        }

        private MessageResource GetMessage(string sid)
        {
            var config = _config.SmsSettings;
            TwilioClient.Init(config.UserName, config.Password);

            return MessageResource.Fetch(pathSid: sid);
        }
    }
}
