using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliSource.Cloud.Core.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ChilliCoreTemplate.Service.EmailAccount
{

    public interface IEmailWorkTask
    {
        Task Execute(ITaskExecutionInfo executionInfo);
    }

    public class EmailWorkTask : IEmailWorkTask
    {
        private readonly Func<ITaskExecutionInfo, IServiceProvider, Task> _task;
        private readonly IServiceProvider _provider;

        private EmailWorkTask(Func<ITaskExecutionInfo, IServiceProvider, Task> task, IServiceProvider provider)
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

        public static IEmailWorkTask EmptyTask(IServiceProvider provider)
        {
            return new EmailWorkTask(null, provider);
        }
        
        public static IEmailWorkTask CreateTask(Func<ITaskExecutionInfo, IServiceProvider, Task> task, IServiceProvider provider)
        {
            return new EmailWorkTask(task, provider);
        }
    }    
   
    
    public interface IEmailQueue
    {
        Task Enqueue(EmailQueueItem<IEmailTemplateDataModel> queuedItem);
        Task Enqueue<T>(EmailQueueItem<T> queuedItem);

        Task<IEmailWorkTask> Dequeue();
    }

    public abstract class EmailQueueBase : IEmailQueue
    {
        protected async Task SaveEmailRecord(IServiceProvider provider , Email record)
        {
            using (var scope = provider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<DataContext>();
                context.Emails.Add(record);
                await context.SaveChangesAsync();
                await AccountService.Activity_AddAsync(context,
                    new UserActivity
                    {
                        ActivityType = ActivityType.Create, UserId = record.UserId.GetValueOrDefault(0),
                        EntityType = EntityType.Email, EntityId = record.Id
                    });
            }
        }

        public abstract Task Enqueue(EmailQueueItem<IEmailTemplateDataModel> queuedItem);


        public abstract Task Enqueue<T>(EmailQueueItem<T> queuedItem);
        public abstract Task<IEmailWorkTask> Dequeue();
    }
}