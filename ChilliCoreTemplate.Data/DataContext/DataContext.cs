using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models.Api;
using ChilliSource.Cloud.Core.Distributed;
using ChilliSource.Cloud.Core.EntityFramework;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Reflection;

namespace ChilliCoreTemplate.Data
{
    public partial class DataContext : DbContext, ITaskRepository, IDataProtectionKeyContext
    {
        public DbSet<DistributedLock> DistributedLocks { get; set; }
        public DbSet<SingleTaskDefinition> SingleTasks { get; set; }
        public DbSet<RecurrentTaskDefinition> RecurrentTasks { get; set; }

        public DbContext DbContext => this;

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        public DataContext() : base()
        {

        }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            DistributedLockSetup.OnModelCreating(modelBuilder);
            TaskDefinitionSetup.OnModelCreating(modelBuilder);
            DateTimeKindAttribute.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.Entity<ApiLogEntry>().HasIndex(e => e.RequestTimestamp);
            modelBuilder.Entity<ApiLogEntry>().HasIndex(e => e.ResponseStatusCode);

            modelBuilder.Entity<ErrorLog>().HasIndex(c => c.TimeStamp);

            modelBuilder.Entity<Webhook_Inbound>().HasIndex(c => c.WebhookIdHash);

            modelBuilder.Entity<Company>().HasIndex(c => c.Guid).HasDatabaseName("IX_Company_Guid").IsUnique();
            modelBuilder.Entity<Company>().HasIndex(c => c.StripeId).IsUnique();
            modelBuilder.Entity<Company>().HasIndex(c => c.ExternalIdHash);

            modelBuilder.Entity<Email>().HasIndex(c => c.TrackingId);
            modelBuilder.Entity<Email>().HasIndex(c => c.TemplateIdHash);
            modelBuilder.Entity<Email>().HasIndex(c => c.DateQueued);

            modelBuilder.Entity<SmsQueueItem>().HasIndex(c => c.TemplateIdHash);
            modelBuilder.Entity<SmsQueueItem>().HasIndex(c => c.MessageIdHash);
            modelBuilder.Entity<SmsQueueItem>().HasIndex(c => c.QueuedOn);

            modelBuilder.Entity<PushNotification>().HasIndex(c => c.CreatedOn);

            modelBuilder.Entity<UserDevice>().HasIndex(c => c.PinToken);

            modelBuilder.Entity<UserSession>().HasIndex(c => c.SessionId);
            modelBuilder.Entity<UserSession>().HasIndex(c => c.SessionExpiryOn);

            modelBuilder.Entity<UserToken>().HasIndex(c => c.Token);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                //Don't cascade during delete
                foreach (var fk in entityType.GetForeignKeys().Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade))
                {
                    fk.DeleteBehavior = DeleteBehavior.Restrict;
                }
            }

            Project_OnModelCreating(modelBuilder);
        }

    }
}
