using ChilliCoreTemplate.Models.Api;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web.MVC;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChilliCoreTemplate.Models.EmailAccount
{
    public class PushNotificationListModel
    {
        public DateTime? DateFrom { get; set; } = DateTime.UtcNow.Date.ToTimezone().AddDays(-7);
        public DateTime? DateTo { get; set; } = DateTime.UtcNow.Date.ToTimezone();

        [EmptyItem("Any type")]
        public PushNotificationType? Type { get; set; }

        [EmptyItem("Sent?")]
        public bool? Sent { get; set; }
        public SelectList SentList => new KeyValuePair<bool, string>[] { new KeyValuePair<bool, string>(true, "Sent"), new KeyValuePair<bool, string>(false, "Unsent") }.ToSelectList(v => v.Key, t => t.Value, true);

        [EmptyItem("Opened?")]
        public bool? Opened { get; set; }
        public SelectList OpenedList => new KeyValuePair<bool, string>[] { new KeyValuePair<bool, string>(true, "Opened"), new KeyValuePair<bool, string>(false, "Unopened") }.ToSelectList(v => v.Key, t => t.Value, true);

        [Placeholder("Search"), MaxLength(100)]
        public string Search { get; set; }
    }

    public class PushNotificationSummaryModel
    {
        public int Id { get; set; }

        public string Type { get; set; }
        [JsonIgnore]
        public PushNotificationType _Type { get; set; }

        public string Recipient { get; set; }

        public DateTime CreatedOn { get; set; }

        public string CreatedOnDisplay { get { return CreatedOn.ToTimezone("Australia/Sydney").ToIsoDateTime(); } }

        [JsonConverter(typeof(StringEnumConverter))]
        public PushNotificationStatus Status { get; set; }

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

    public class PushNotificationDevice
    {
        public int UserId { get; set; }

        public int UserDeviceId { get; set; }

        public string TokenId { get; set; }

        public PushNotificationProvider Provider { get; set; }
    }
}
