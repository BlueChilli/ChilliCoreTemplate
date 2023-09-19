using ChilliSource.Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Models.Api
{
    public class SendNotificationModel : SendNotificationApiModel
    {
        public string PushTokenId { get; set; }
        public PushNotificationProvider Provider { get; set; }
        public PushNotificationAppId AppId { get; set; }
        public int? UserId { get; set; }
        public int? UserDeviceId { get; set; }
    }

    public class SendNotificationApiModel
    {
        public SendNotificationApiModel()
        {
            Sound = "default";
        }

        [Required]
        public PushNotificationType Type { get; set; }

        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        public string Sound { get; set; }

        public int? BadgeCount { get; set; }

        public Dictionary<string, string> Data { get; set; }
    }

    public enum PushNotificationProvider
    {
        [Data("Provider", "APNS")]
        [Data("Sandbox", "APNS_SANDBOX")]
        Apple = 1,
        [Data("Provider", "GCM")]
        [Data("Sandbox", "GCM")]
        Google,
        FireBase
    };

    public enum PushNotificationAppId
    {
        Default = 1
    }

    public enum PushNotificationType
    {
        Test
    }

    public enum PushNotificationStatus
    {
        Initialising = 1,
        Queued,
        Sent,
        Error,
        QueuedInternally
    }

}
