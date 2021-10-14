using ChilliCoreTemplate.Models;
using ChilliSource.Core.Extensions;
using System;
using System.ComponentModel.DataAnnotations;

namespace ChilliCoreTemplate.Data.EmailAccount
{
    public class SmsQueueItem
    {
        public SmsQueueItem() { }

        public SmsQueueItem(RazorTemplate template)
        {
            TemplateId = template.TemplateName;
            TemplateIdHash = TemplateId.GetIndependentHashCode() ?? template.GetHashCode();
        }

        public int Id { get; set; }

        public int? UserId { get; set; }
        public User User { get; set; }

        [StringLength(50)]
        public string TemplateId { get; set; }

        public int TemplateIdHash { get; set; }

        [StringLength(1000)]
        public string Data { get; set; }

        [MaxLength(50)]
        public string MessageId { get; set; }

        public int MessageIdHash { get; set; }

        public DateTime QueuedOn { get; set; }

        public bool IsReady { get; set; }

        public DateTime? SentOn { get; set; }

        public DateTime? DeliveredOn { get; set; }

        public DateTime? ClickedOn { get; set; }

        public string Error { get; set; }
        
        public int? RetryCount { get; set; }
    }


}
