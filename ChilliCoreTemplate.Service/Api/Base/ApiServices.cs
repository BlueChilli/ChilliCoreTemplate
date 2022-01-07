using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChilliCoreTemplate.Models;
using System.Security.Principal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace ChilliCoreTemplate.Service.Api
{
    public partial class ApiServices : Service<DataContext>
    {
        private readonly Services _services;
        private readonly AccountService _accountService;
        private readonly StripeService _stripe;
        private readonly ProjectSettings _config;
        private readonly IFileStorage _fileStorage;
        private readonly FileStoragePath _fileStoragePath;
        private readonly IWebHostEnvironment _environment;

        public ApiServices(IPrincipal user, DataContext context, Services services, AccountService accountService, StripeService stripe, ProjectSettings config, IFileStorage fileStorage, FileStoragePath fileStoragePath, IWebHostEnvironment environment) : base(user, context)
        {
            _services = services;
            _accountService = accountService;
            _stripe = stripe;
            _config = config;
            _fileStorage = fileStorage;
            _fileStoragePath = fileStoragePath;
            _environment = environment;
        }

        public Task Api_Log_SaveAsync(ApiLogEntry model)
        {
            Context.ApiLogEntries.Add(model);
            return Context.SaveChangesAsync();
        }

        public async Task Api_Log_Clean(ITaskExecutionInfo executionInfo)
        {
            executionInfo.SendAliveSignal();
            if (executionInfo.IsCancellationRequested)
                return;

            await Context.Database.ExecuteSqlRawAsync($"DELETE FROM[dbo].[ApiLogEntries] WHERE ID IN(SELECT TOP(50) Id FROM [dbo].[ApiLogEntries]) AND [RequestTimestamp] < DATEADD(day, -14, SYSUTCDATETIME())");
        }
    }
}
