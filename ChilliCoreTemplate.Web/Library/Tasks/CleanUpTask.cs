using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service;
using ChilliCoreTemplate.Service.Api;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.Tasks
{
    public class CleanUpTask : IDistributedTaskAsync<object>
    {
        public async Task RunAsync(object parameter, ITaskExecutionInfo executionInfo)
        {
            using (var scope = ScopeContextFactory.Instance.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<UserSessionService>();

                await svc.Clean(executionInfo);
            }

            using (var scope = ScopeContextFactory.Instance.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<ApiLogService>();

                await svc.Clean(executionInfo);
            }

            using (var scope = ScopeContextFactory.Instance.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<AccountService>();

                await svc.Error_CleanAsync(executionInfo);
                await svc.Anonymous_CleanAsync(executionInfo);
            }

            using (var scope = ScopeContextFactory.Instance.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<WebhookService>();

                await svc.CleanWebhooks(executionInfo);
            }

            using (var scope = ScopeContextFactory.Instance.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<BulkImportService>();

                await svc.CleanUp(executionInfo);
            }

            using (var scope = ScopeContextFactory.Instance.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<SystemService>();

                svc.DocumentCache_CleanUp(executionInfo);
            }
        }

        public Task RunAsync(object parameter, ITaskExecutionInfoAsync executionInfo)
        {
            throw new NotImplementedException();
        }
    }
}
