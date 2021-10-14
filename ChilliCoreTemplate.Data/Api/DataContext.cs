using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models.Api;
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
        public DbSet<ApiLogEntry> ApiLogEntries { get; set; }
    }
}
