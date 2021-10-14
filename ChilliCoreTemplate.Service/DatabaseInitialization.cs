
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Core.Distributed;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data.Common;

namespace ChilliCoreTemplate.Service
{
    public class DatabaseInitialization
    {
        private static readonly Guid DB_MIGRATION_RESOURCE = new Guid("8A047FA6-7205-465C-9A44-6D7DDD1415AD");

        IServiceProvider _serviceProvider;

        public DatabaseInitialization(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static void Initialize(IServiceProvider serviceProvider)
        {
            new DatabaseInitialization(serviceProvider).Initialize();
        }

        public void Initialize()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var config = scope.ServiceProvider.GetRequiredService<ProjectSettings>();
                var hosting = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

                if (!DatabaseExists() || !LockTableExists())
                {
                    var context = scope.ServiceProvider.GetRequiredService<DataContext>();                    
                    context.Database.Migrate();

                    //lock manager needs to be created after database creation.
                    var lockManager = scope.ServiceProvider.GetRequiredService<ILockManager>();
                    lockManager.RunWithLock(DB_MIGRATION_RESOURCE, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2),
                        (_lock) =>
                        {
                            new DataSeed(config, hosting).Run(context);
                            context.SaveChanges();
                        });
                }
                else
                {

                    var lockManager = scope.ServiceProvider.GetRequiredService<ILockManager>();
                    lockManager.RunWithLock(DB_MIGRATION_RESOURCE, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2),
                        (_lock) =>
                        {
                            var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                            context.Database.Migrate();

                            new DataSeed(config, hosting).Run(context);
                            context.SaveChanges();
                        });
                }
            }
        }

        public bool DatabaseExists()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                return (context.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator).Exists();
            }
        }

        private bool LockTableExists()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                int result;
                var connection = context.Database.GetDbConnection();
                try
                {
                    if (connection.State != System.Data.ConnectionState.Open)
                        connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "select COUNT(*) from INFORMATION_SCHEMA.TABLES where Table_Name = 'DistributedLocks'";
                        result = (int)cmd.ExecuteScalar();
                    }
                }
                finally
                {
                    if (connection.State != System.Data.ConnectionState.Closed)
                        connection.Close();
                }

                return result > 0;
            }
        }
    }
}
