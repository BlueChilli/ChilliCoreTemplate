using ChilliCoreTemplate.Models.Api;
using ChilliSource.Cloud.Core;
using ChilliSource.Core.Extensions;
using Newtonsoft.Json;
using System;

namespace ChilliCoreTemplate.Models.EmailAccount
{
    public class PushNotificationSummaryModel
    {
        public int Id { get; set; }

        public string Type { get; set; }
        [JsonIgnore]
        public PushNotificationType _Type { get; set; }

        public string Recipient { get; set; }

        public DateTime CreatedOn { get; set; }

        public string CreatedOnDisplay { get { return CreatedOn.ToTimezone("Australia/Sydney").ToIsoDateTime(); } }

        public string Status { get; set; }

        public bool IsSent { get; set; }

        public bool IsOpened { get; set; }

        public string Error { get; set; }

    }

    public class PushNotificationDetailModel
    {
        public int Id { get; set; }

        public string Message { get; set; }

        public string MessageId { get; set; }

    }
}
