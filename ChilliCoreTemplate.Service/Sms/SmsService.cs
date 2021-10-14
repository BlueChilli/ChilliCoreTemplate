using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service.Sms;
using ChilliSource.Cloud.Core;
using ChilliSource.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service.EmailAccount
{
    public class SmsService : Service<DataContext>
    {
        private readonly ProjectSettings _config;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;

        public SmsService(
            IPrincipal user,
            DataContext context,
            ProjectSettings config,
            IBackgroundTaskQueue backgroundTaskQueue) : base(user, context)
        {
            _config = config;
            _backgroundTaskQueue = backgroundTaskQueue;
        }

        public void Queue<T>(RazorTemplate template, int userId, string phone, RazorTemplateDataModel<T> model)
        {
            //Sms message generation and persistence happens in the background, so we can return to the caller immediately
            _backgroundTaskQueue.QueueBackgroundWorkItem(async (ct) => await QueueInternal(template, userId, phone, model));
        }


        private static async Task QueueInternal<T>(RazorTemplate template, int userId, string phone, RazorTemplateDataModel<T> model)
        {
            using (var scope = ScopeContextFactory.Instance.CreateScope())
            {
                var queue = scope.ServiceProvider.GetRequiredService<ISmsQueue>();
                await queue.Enqueue(template, userId, phone, model);
            }
        }
    }
}
