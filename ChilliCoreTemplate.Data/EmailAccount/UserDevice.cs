using ChilliCoreTemplate.Models.Api;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Data.EmailAccount
{
    public class UserDevice
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string DeviceId { get; set; }

        public virtual User User { get; set; }
        public int UserId { get; set; }

        [Required]
        public Guid PinToken { get; set; }

        [StringLength(200)]
        public string PinHash { get; set; }
        public int PinRetries { get; set; }
        public DateTime? PinLastRetryDate { get; set; }

        #region Push
        [StringLength(200)]
        public string PushToken { get; set; }

        [StringLength(500)]
        public string PushTokenId { get; set; }

        public PushNotificationProvider? PushProvider { get; set; }

        public PushNotificationAppId? PushAppId { get; set; }
        #endregion

        public virtual List<UserSession> UserSessions { get; set; }
    }
}
