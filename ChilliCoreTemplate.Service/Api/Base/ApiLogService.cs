using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.Distributed;
using ChilliSource.Cloud.Core.LinqMapper;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service.Api
{
    public class ApiLogService : Service<DataContext>
    {
        public ApiLogService(BackgroundTaskPrincipal user, DataContext context) : base(user, context)
        {
        }

        public Task SaveAsync(ApiLogEntry model)
        {
            Context.ApiLogEntries.Add(model);
            return Context.SaveChangesAsync();
        }

        public async Task Clean(ITaskExecutionInfo executionInfo)
        {
            if (executionInfo != null)
            {
                executionInfo.SendAliveSignal();
                if (executionInfo.IsCancellationRequested)
                    return;
            }

            await Context.Database.ExecuteSqlRawAsync($"DELETE FROM[dbo].[ApiLogEntries] WHERE Id IN (SELECT TOP(50) Id FROM [dbo].[ApiLogEntries] ORDER BY Id) AND [RequestTimestamp] < DATEADD(day, -14, SYSUTCDATETIME())");
        }
    }

}
