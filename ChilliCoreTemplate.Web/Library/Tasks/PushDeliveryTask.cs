using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Service;
using ChilliCoreTemplate.Service.Api;
using ChilliCoreTemplate.Service.Api.PushNotifications;
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
                var pushConfig = scope.ServiceProvider.GetRequiredService<PushNotificationServiceFactory>();
                var pushService = pushConfig.GetService(PushNotificationAppId.Default);
                await pushService.QueuePushNotificationTask(executionInfo);
            }
        }
    }
}
