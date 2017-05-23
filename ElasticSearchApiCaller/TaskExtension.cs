using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ElasticSearchApiCaller
{
    public static class TaskExtension
    {
        // http://stackoverflow.com/questions/23316444/await-multiple-async-task-while-setting-max-running-task-at-a-time

        public static async Task WhenAllDone(this IEnumerable<Func<Task>> actions, int threadCount)
        {
            var countdownEvent = new CountdownEvent(actions.Count());
            var _throttler = new SemaphoreSlim(threadCount);

            foreach (Func<Task> action in actions)
            {
                await _throttler.WaitAsync();

                await Task.Run(async () =>
                {
                    try
                    {
                        await action();
                    }
                    finally
                    {
                        _throttler.Release();
                        countdownEvent.Signal();
                    }
                });
            }

            countdownEvent.Wait();
        }
    }
}