using ChilliCoreTemplate.Models.Admin;
using ChilliSource.Core.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilliCoreTemplate.Models.Sms
{
    public class SmsMessageViewModel
    {
        public string To { get; set; }  //Mobile number or if sending via email the email address
        public string Message { get; set; }
        public string From { get; set; }
        public string Url { get; set; }

    }

    public class SmsViewModel
    {
        public SmsMessageViewModel Data => _Data == null ? null : _Data.FromJson<SmsMessageViewModel>();

        [JsonIgnore]
        public string _Data { get; set; }

        public string Error { get; set; }

    }

    public class SmsSummaryModel
    {
        public int Id { get; set; }

        [JsonIgnore]
        public UserSummaryViewModel User { get; set; }
        public string UserDisplay => String.IsNullOrEmpty(User.FullName) ? User.Phone : User.FullName;
        public int UserId => User.Id;

        [JsonIgnore]
        public DateTime QueuedOn { get; set; }
        public string QueuedOnDisplay { get; set; }

        [JsonIgnore]
        public string _Data { get; set; }
        public SmsMessageViewModel Data => _Data == null ? null : _Data.FromJson<SmsMessageViewModel>();

        public string TemplateId { get; set; }

        public bool IsSent { get; set; }
        public bool IsDelivered { get; set; }
        public bool IsClicked { get; set; }
    }
}
