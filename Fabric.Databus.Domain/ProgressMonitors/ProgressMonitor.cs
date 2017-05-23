using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ElasticSearchSqlFeeder.Interfaces;
using ElasticSearchSqlFeeder.ProgressMonitor;

namespace Fabric.Databus.Domain.ProgressMonitors
{
    public class ProgressMonitor : IProgressMonitor, IDisposable
    {
        private readonly ConcurrentDictionary<int, ProgressMonitorItem> _progressMonitorItems =
            new ConcurrentDictionary<int, ProgressMonitorItem>();

        private static readonly object Lock = new object();

        private static int _id = 0;

        private readonly Task _backgroundTask;

        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly IProgressLogger _progressLogger;

        public Action JobHistoryUpdateAction { get; set; }


        public ProgressMonitor() : this(null)
        {
        }

        public ProgressMonitor(IProgressLogger progressLogger)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            _progressLogger = progressLogger;

            if (_progressLogger != null)
            {
                _backgroundTask = Task.Run(() => LogProgress(token), token);
            }
        }

        public void LogProgress(CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                _progressLogger.Reset();

                //Process proc = Process.GetCurrentProcess();
                //Console.WriteLine($"Memory: {(proc.WorkingSet64 / 1024):N0} kb");

                var progressMonitorItems = _progressMonitorItems.OrderBy(p => p.Key).Select(p => new
                    {
                        p.Key,
                        p.Value
                    })
                    .ToList();

                foreach (var progressMonitorItem in progressMonitorItems)
                {
                    _progressLogger.LogProgressMonitorItem(progressMonitorItem.Key, progressMonitorItem.Value);
                }

                Thread.Sleep(1000);
            }
        }

        public void SetProgressItem(ProgressMonitorItem progressMonitorItem)
        {

            //lock (Lock)
            {
                Interlocked.Increment(ref _id);
                var key = progressMonitorItem.StepNumber;
                _progressMonitorItems.AddOrUpdate(key, progressMonitorItem, (a, b) => progressMonitorItem);
                JobHistoryUpdateAction?.Invoke();
            }

        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
        }

        public IList<ProgressMonitorItem> GetSnapshotOfProgressItems()
        {
            return _progressMonitorItems.ToList().Select(a => a.Value).ToList();
        }
    }
}