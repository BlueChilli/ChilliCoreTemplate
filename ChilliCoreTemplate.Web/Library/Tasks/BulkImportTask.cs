using ChilliCoreTemplate.Service;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.Tasks
{
    public class BulkImportTask : IDistributedTaskAsync<object>
    {
        public async Task RunAsync(object parameter, ITaskExecutionInfoAsync executionInfo)
        {

            using var scope = ScopeContextFactory.Instance.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<BulkImportService>();

            await svc.Execute(executionInfo);
        }
    }
}
