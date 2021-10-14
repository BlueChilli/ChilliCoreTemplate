using ChilliCoreTemplate.Data.EmailAccount;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Data
{
    public partial class DataContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }
        public DbSet<UserDevice> UserDevices { get; set; }

        public DbSet<UserActivity> UserActivities { get; set; }
        public DbSet<Email> Emails { get; set; }
        public DbSet<EmailUser> EmailUsers { get; set; }
        public DbSet<SmsQueueItem> SmsQueue { get; set; }
        public DbSet<PushNotification> PushNotifications { get; set; }

        public DbSet<Company> Companies { get; set; }

        public DbSet<ErrorLog> ErrorLogs { get; set; }

    }
}
