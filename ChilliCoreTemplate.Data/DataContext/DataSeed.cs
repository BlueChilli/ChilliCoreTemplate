using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Core;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChilliCoreTemplate.Data
{
    public class DataSeed
    {
        ProjectSettings _config;
        IWebHostEnvironment _env;
        public DataSeed(ProjectSettings config, IWebHostEnvironment env)
        {
            _config = config;
            _env = env;
        }

        public void Run(DataContext context)
        {
            var adminEmail = $"{_env.EnvironmentName.ToLower()}@{new Uri(_config.PublicUrl).Domain()}";
            if (_env.IsProduction())
            {
                //Note password will be the project guid in lowercase
                adminEmail = $"admin@{new Uri(_config.PublicUrl).Domain()}";
                AddAdmin(context, adminEmail, _config.ProjectId.Value.ToString());
            }
            else
            {
                AddAdmin(context, adminEmail, "123456");
            }
        }

        private void AddAdmin(DataContext context, string adminEmail, string password)
        {
            var salt = Guid.NewGuid();
            if (!context.Users.Any(a => a.Email == adminEmail))
            {
                var adminAccount = context.Users.Add(
                    new User()
                    {
                        Email = adminEmail,
                        FirstName = "Admin",
                        LastName = _config.ProjectName,
                        PasswordSalt = salt,
                        PasswordHash = password.SaltedHash($"{salt}{_config.ProjectId}"),
                        Status = UserStatus.Activated,
                        ActivatedDate = DateTime.UtcNow,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow
                    }).Entity;

                adminAccount.UserRoles = new List<UserRole>() { new UserRole() { User = adminAccount, CreatedAt = DateTime.UtcNow, Role = Role.Administrator } };
            }
        }
    }
}
