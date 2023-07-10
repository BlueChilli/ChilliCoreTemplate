using ChilliCoreTemplate.Service;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace ChilliCoreTemplate.Web.Tasks
{
    public class BulkImportTask : IDistributedTask<object>
    {
        public void Run(object parameter, ITaskExecutionInfo executionInfo)
        {

            TaskHelper.WaitSafeSync(async () =>
            {
                using (var scope = ScopeContextFactory.Instance.CreateScope())
                {
                    var svc = scope.ServiceProvider.GetRequiredService<BulkImportService>();
                    
                    await svc.Execute(executionInfo);
                    await svc.CleanUp(executionInfo);
                }
            });
           
        }
    }
}
