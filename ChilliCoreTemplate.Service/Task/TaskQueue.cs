using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service
{
    public class TaskQueue
    {
        private SemaphoreSlim semaphore;
        public TaskQueue()
        {
            semaphore = new SemaphoreSlim(1);
        }
        public TaskQueue(int concurrentRequests)
        {
            semaphore = new SemaphoreSlim(concurrentRequests);
        }

        public async Task<T> Enqueue<T>(Func<Task<T>> taskGenerator)
        {
            await semaphore.WaitAsync();
            try
            {
                return await taskGenerator();
            }
            finally
            {
                semaphore.Release();
            }
        }
        public async Task Enqueue(Func<Task> taskGenerator)
        {
            await semaphore.WaitAsync();
            try
            {
                await taskGenerator();
            }
            finally
            {
                semaphore.Release();
            }
        }
    }

    public static class TaskQueueHelpers
    {
        public static void Match<T>(this TaskCompletionSource<T> tcs, Task<T> task)
        {
            task.ContinueWith(t =>
            {
                switch (t.Status)
                {
                    case TaskStatus.Canceled:
                        tcs.SetCanceled();
                        break;
                    case TaskStatus.Faulted:
                        tcs.SetException(t.Exception.InnerExceptions);
                        break;
                    case TaskStatus.RanToCompletion:
                        tcs.SetResult(t.Result);
                        break;
                }

            });
        }

        public static void Match<T>(this TaskCompletionSource<T> tcs, Task task)
        {
            Match(tcs, task.ContinueWith(t => default(T)));
        }
    }

    public class TaskThrottler
    {
        private TaskQueue queue;
        public TaskThrottler(int requestsPerSecond)
        {
            queue = new TaskQueue(requestsPerSecond);
        }

        public Task<T> Enqueue<T>(Func<Task<T>> taskGenerator)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            var unused = queue.Enqueue(() =>
            {
                tcs.Match(taskGenerator());
                return Task.Delay(1000);
            });
            return tcs.Task;
        }

        public Task Enqueue(Func<Task> taskGenerator)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            var unused = queue.Enqueue(() =>
            {
                tcs.Match(taskGenerator());
                return Task.Delay(1000);
            });
            return tcs.Task;
        }
    }
}
