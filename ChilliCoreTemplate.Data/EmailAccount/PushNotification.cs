using ChilliCoreTemplate.Models.Api;
using ChilliSource.Cloud.Core.EntityFramework;
using System;
using System.ComponentModel.DataAnnotations;

namespace ChilliCoreTemplate.Data.EmailAccount
{
    public class PushNotification
    {

        public int Id { get; set; }

        public Guid TrackingId { get; set; }

        public int? UserId { get; set; }
        public User User { get; set; }

        public PushNotificationType Type { get; set; }

        public PushNotificationProvider Provider { get; set; }

        [MaxLength(100)]
        public string MessageId { get; set; }

        public string Message { get; set; }

        [DateTimeKind]
        public DateTime CreatedOn { get; set; }

        [DateTimeKind]
        public DateTime? OpenedOn { get; set; }

        public PushNotificationStatus Status { get; set; }

        public string Error { get; set; }

        public bool IsQueued => Status >= PushNotificationStatus.Queued;

        public bool IsSent => Status >= PushNotificationStatus.Sent;

        public bool IsOpened => OpenedOn.HasValue;


        public static PushNotification CreateFrom(SendNotificationModel model)
        {
            return new PushNotification
            {
                UserId = model.UserId,
                CreatedOn = DateTime.UtcNow,
                Status = PushNotificationStatus.Initialising,
                TrackingId = Guid.NewGuid(),
                Type = model.Type,
                Provider = model.Provider
            };
        }

    }
}
