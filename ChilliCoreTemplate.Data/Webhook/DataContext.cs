using Microsoft.EntityFrameworkCore;

namespace ChilliCoreTemplate.Data
{
    public partial class DataContext
    {
        public DbSet<Webhook_Inbound> Webhooks_Inbound { get; set; }

        //public DbSet<Webhook_Outbound> Webhooks_Outbound { get; set; }
    }
}
