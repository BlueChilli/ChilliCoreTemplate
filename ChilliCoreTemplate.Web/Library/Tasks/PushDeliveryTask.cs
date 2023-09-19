using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Service;
using ChilliCoreTemplate.Service.Api;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.Tasks
{
    public class PushDeliveryTask : IDistributedTaskAsync<object>
    {
        public async Task RunAsync(object parameter, ITaskExecutionInfoAsync executionInfo)
        {
            using (var scope = ScopeContextFactory.Instance.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<WebhookService>();

                var pushConfig = scope.ServiceProvider.GetRequiredService<PushNotificationConfiguration>();
                var pushService = pushConfig.GetService(PushNotificationAppId.Default);
                await pushService.QueuePushNotificationTask(executionInfo);
            }
        }
    }
}
