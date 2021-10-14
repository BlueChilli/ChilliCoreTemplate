using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.Distributed;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Data
{
    public partial class DataContext : DbContext
    {
        public DbSet<Location> Locations { get; set; }
        public DbSet<LocationUser> LocationUsers { get; set; }

        //public DbSet<Payout> Payouts { get; set; }

        private void Project_OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LocationUser>()
                .HasIndex(nameof(LocationUser.LocationId), nameof(LocationUser.UserId))
                .IsUnique();
        }

    }
}
