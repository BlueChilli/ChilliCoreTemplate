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
        public DbSet<Payment> Payments { get; set; }

        private void Project_OnModelCreating(ModelBuilder modelBuilder)
        {
        }

    }
}
