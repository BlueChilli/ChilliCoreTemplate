using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.Distributed;
using Microsoft.Extensions.DependencyInjection;
using ChilliCoreTemplate.Service;
using ChilliCoreTemplate.Service.EmailAccount;

namespace ChilliCoreTemplate.Web.Tasks
{
    public class ErrorLogTask : IDistributedTask<object>
    {
        public void Run(object parameter, ITaskExecutionInfo executionInfo)
        {
            using (var scope = ScopeContextFactory.Instance.CreateScope())
            {

                var svc = scope.ServiceProvider.GetRequiredService<AccountService>();

                TaskHelper.WaitSafeSync(() => svc.Error_EmailAsync(executionInfo));
            }
        }
    }

}