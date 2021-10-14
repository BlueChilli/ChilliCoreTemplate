using System;
using System.ComponentModel.DataAnnotations;

namespace ChilliCoreTemplate.Models.Api
{
    public class WebhookApiModel
    {
        public int Id { get; set; }

        public WebhookEvent Event { get; set; }

        [StringLength(100)]
        public string Target_Url { get; set; }

        public Guid ApiKey { get; set; }
    }

    public enum WebhookEvent
    {
        Dog_Created = 1,
        Cat_Created,
        Cat_FirstBirthday,
        Cat_Cleaned
    }

    public enum WebhookType
    {
        Stripe = 1,
        Sns,
        Twilio
    }

}
