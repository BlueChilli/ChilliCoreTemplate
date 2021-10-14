
using ChilliCoreTemplate.Models.Api;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChilliCoreTemplate.Data
{
    [Table("Webhooks_Inbound")]
    public class Webhook_Inbound
    {
        public int Id { get; set; }

        [StringLength(100)]
        public string WebhookId { get; set; }

        public int WebhookIdHash { get; set; }

        public WebhookType Type { get; set; }

        [StringLength(100)]
        public string Subtype { get; set; }

        public string Raw { get; set; }

        public string Error { get; set; }

        public bool Success { get; set; }

        public bool Processed { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
