using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliSource.Cloud.Core.EntityFramework;
using ChilliSource.Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Data.EmailAccount
{
    public class Email
    {
        public Email() { }

        public Email(RazorTemplate template)
        {
            TemplateId = template.TemplateName;
            TemplateIdHash = TemplateId.GetIndependentHashCode() ?? template.GetHashCode();
        }

        public int Id { get; set; }

        public Guid TrackingId { get; set; }

        public int? UserId { get; set; }
        public User User { get; set; }

        [StringLength(100)]
        public string TemplateId { get; set; }

        public int TemplateIdHash { get; set; }

        [StringLength(100)]
        public string Recipient { get; set; }

        public string Model { get; set; }

        [StringLength(100)]
        public string Attachments { get; set; }

        [DateTimeKind]
        public DateTime DateQueued { get; set; }

        public bool IsReady { get; set; }

        public bool IsSent { get; set; }

        public bool IsSending { get; set; }

        [DateTimeKind]
        public DateTime? DateSent { get; set; }

        public bool IsOpened { get; set; }

        [DateTimeKind]
        public DateTime? OpenDate { get; set; }

        public int OpenCount { get; set; }

        public bool IsClicked { get; set; }

        [DateTimeKind]
        public DateTime? ClickDate { get; set; }

        public int ClickCount { get; set; }

        public bool IsUnsubscribed { get; set; }

        [DateTimeKind]
        public DateTime? UnsubscribeDate { get; set; }

        public string Error { get; set; }
        
        public int? RetryCount { get; set; }
    }

    public class EmailUser
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public string TemplateId { get; set; }

        public int TemplateIdHash { get; set; }

        public bool IsUnsubscribed { get; set; }

        public DateTime? UnsubscribeDate { get; set; }

        public EmailUnsubscribeReason? Reason { get; set; }

        [MaxLength(200)]
        public string ReasonOther { get; set; }
    }

}
