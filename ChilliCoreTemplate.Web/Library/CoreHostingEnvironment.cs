using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web
{
    public class CoreHostingEnvironment : ChilliSource.Cloud.Core.IHostingEnvironment
    {
        IBackgroundTaskQueue _taskQueue;
        public CoreHostingEnvironment(IBackgroundTaskQueue taskQueue)
        {
            _taskQueue = taskQueue;
        }

        public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
        {
            _taskQueue.QueueBackgroundWorkItem(workItem);
        }

        public void QueueBackgroundWorkItem(Action<CancellationToken> workItem)
        {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            _taskQueue.QueueBackgroundWorkItem(async (c) => workItem(c));
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        }
    }
}
