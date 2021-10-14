using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ChilliCoreTemplate.Web.Tasks
{
    public class EmailDeliveryTask : IDistributedTask<object>
    {
        public void Run(object parameter, ITaskExecutionInfo executionInfo)
        {
            TaskHelper.WaitSafeSync(async () =>
            {
                try
                {
                    using (var scope = ScopeContextFactory.Instance.CreateScope())
                    {
                        var emailQueue = scope.ServiceProvider.GetRequiredService<IEmailQueue>();
                        var workItem = await emailQueue.Dequeue();

                        await workItem.Execute(executionInfo);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            });
        }
    }
}
