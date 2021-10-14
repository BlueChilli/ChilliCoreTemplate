using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliCoreTemplate.Service.Sms;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ChilliCoreTemplate.Web
{
    public class SmsDeliveryTask : IDistributedTask<object>
    {
        public void Run(object parameter, ITaskExecutionInfo executionInfo)
        {
            TaskHelper.WaitSafeSync(async () =>
            {
                try
                {
                    using (var scope = ScopeContextFactory.Instance.CreateScope())
                    {
                        var smsQueue = scope.ServiceProvider.GetRequiredService<ISmsQueue>();
                        await smsQueue.Process(executionInfo);
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
