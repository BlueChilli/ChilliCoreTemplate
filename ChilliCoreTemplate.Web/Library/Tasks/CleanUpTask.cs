using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service;
using ChilliCoreTemplate.Service.Api;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ChilliCoreTemplate.Web.Tasks
{
    public class CleanUpTask : IDistributedTask<object>
    {
        public void Run(object parameter, ITaskExecutionInfo executionInfo)
        {
            using (var scope = ScopeContextFactory.Instance.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<UserSessionService>();

                svc.Clean(executionInfo);
            }

            TaskHelper.WaitSafeSync(async () =>
            {
                using (var scope = ScopeContextFactory.Instance.CreateScope())
                {
                    var svc = scope.ServiceProvider.GetRequiredService<ApiServices>();

                    await svc.Api_Log_Clean(executionInfo);
                }
            });

            TaskHelper.WaitSafeSync(async () =>
            {
                using (var scope = ScopeContextFactory.Instance.CreateScope())
                {
                    var svc = scope.ServiceProvider.GetRequiredService<AccountService>();

                    await svc.Error_CleanAsync(executionInfo);
                    await svc.Anonymous_CleanAsync(executionInfo);
                }
            });

            TaskHelper.WaitSafeSync(async () =>
            {
                using (var scope = ScopeContextFactory.Instance.CreateScope())
                {
                    var svc = scope.ServiceProvider.GetRequiredService<WebhookService>();

                    await svc.CleanWebhooks(executionInfo);
                }
            });
        }
    }
}
