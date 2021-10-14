using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliSource.Cloud.Core.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service.Sms
{
    public interface ISmsQueue
    {
        Task Enqueue<T>(RazorTemplate template, int userId, string phone, RazorTemplateDataModel<T> model);

        Task Process(ITaskExecutionInfo executionInfo);
    }

    public interface ISmsWorkTask
    {
        Task Execute(ITaskExecutionInfo executionInfo);
    }

    public class SmsWorkTask : ISmsWorkTask
    {
        private readonly Func<ITaskExecutionInfo, IServiceProvider, Task> _task;
        private readonly IServiceProvider _provider;

        private SmsWorkTask(Func<ITaskExecutionInfo, IServiceProvider, Task> task, IServiceProvider provider)
        {
            _task = task;
            _provider = provider;
        }

        public async Task Execute(ITaskExecutionInfo info)
        {
            if (_task != null)
            {
                await _task.Invoke(info, _provider);
            }
        }

        public static ISmsWorkTask EmptyTask(IServiceProvider provider)
        {
            return new SmsWorkTask(null, provider);
        }

        public static ISmsWorkTask CreateTask(Func<ITaskExecutionInfo, IServiceProvider, Task> task, IServiceProvider provider)
        {
            return new SmsWorkTask(task, provider);
        }
    }

    public abstract class SmsQueueBase : ISmsQueue
    {
        protected async Task SaveSmsItem(IServiceProvider provider, SmsQueueItem item)
        {
            using (var scope = provider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<DataContext>();
                context.SmsQueue.Add(item);
                await context.SaveChangesAsync();
                await AccountService.Activity_AddAsync(context,
                    new UserActivity
                    {
                        ActivityType = ActivityType.Create,
                        UserId = item.UserId.GetValueOrDefault(0),
                        EntityType = EntityType.Sms,
                        EntityId = item.Id
                    });
            }
        }
        public abstract Task Enqueue<T>(RazorTemplate template, int userId, string phone, RazorTemplateDataModel<T> model);

        public abstract Task Process(ITaskExecutionInfo executionInfo);
    }
}
