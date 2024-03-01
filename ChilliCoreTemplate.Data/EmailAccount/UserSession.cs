using ChilliCoreTemplate.Data.EmailAccount;
using ChilliSource.Cloud.Core.EntityFramework;
using ChilliSource.Core.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChilliCoreTemplate.Data
{
    public class UserSession
    {
        public long Id { get; set; }

        public virtual User User { get; set; }
        public int UserId { get; set; }

        public virtual UserDevice UserDevice { get; set; }
        public int? UserDeviceId { get; set; }

        [Required]
        public Guid SessionId { get; set; }

        [DateTimeKind]
        public DateTime SessionCreatedOn { get; set; }

        /// <summary>
        /// So session can be cleaned up if session is not explictly terminated (for devices no longer used)
        /// </summary>
        [DateTimeKind]
        public DateTime SessionExpiryOn { get; set; }

        public string ImpersonationChain { get; set; }

        public bool IsMfaVerified { get; set; }
    }
}
