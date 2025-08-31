using Microsoft.EntityFrameworkCore;

namespace ChilliCoreTemplate.Data
{
    public partial class DataContext
    {
        public DbSet<WebhookInbound> WebhooksInbound { get; set; }

        //public DbSet<Webhook_Outbound> Webhooks_Outbound { get; set; }
    }
}
