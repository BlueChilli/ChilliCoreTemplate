using ChilliCoreTemplate.Service;
using ChilliCoreTemplate.Service.Api;
using ChilliSource.Cloud.Core.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.Tasks
{
    public class WebhookTask : IDistributedTaskAsync<object>
    {
        public async Task RunAsync(object parameter, ITaskExecutionInfoAsync executionInfo)
        {
            using (var scope = ScopeContextFactory.Instance.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<WebhookService>();

                await svc.ProcessWebhook(executionInfo);
            }
        }
    }
}
