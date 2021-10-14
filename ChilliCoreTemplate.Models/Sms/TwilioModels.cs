using System;
using System.Collections.Generic;
using System.Text;

namespace ChilliCoreTemplate.Models.Sms
{
    public class TwilioSmsBaseModel
    {
        public TwilioSmsType Type { get; set; }
        public string MessageSid { get; set; }
        public string SmsSid { get; set; }
        public string AccountSid { get; set; }
        public string MessagingServiceSid { get; set; }
        public string From { get; set; }
        public string To { get; set; }
    }

    public class TwilioSmsStatusModel : TwilioSmsBaseModel
    {
        public TwilioSmsStatus MessageStatus { get; set; }
        public TwilioSmsStatus SmsStatus { get; set; }
        public string ApiVersion { get; set; }
    }

    public enum TwilioSmsType
    {
        Status,
        Message
    }

    public enum TwilioSmsStatus
    {
        Accepted,
        Queued,
        Sending,
        Sent,
        Failed,
        Delivered,
        Undelivered
    }

    //https://www.twilio.com/docs/sms/twiml#twilios-request-to-your-application
    public class TwilioSmsMessageModel : TwilioSmsBaseModel
    {
        public string Body { get; set; }
        public int NumMedia { get; set; }

        public TwilioMessageCommand Command => String.IsNullOrWhiteSpace(Body) ? TwilioMessageCommand.None
            : Body.Trim().ToUpper() == "STOP" ? TwilioMessageCommand.Stop
            : Body.Trim().ToUpper() == "START" ? TwilioMessageCommand.Start
            : TwilioMessageCommand.None;
    }

    public enum TwilioMessageCommand
    {
        None,
        Stop,
        Start
    }
}
