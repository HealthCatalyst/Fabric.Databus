// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProgressMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ProgressMonitor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Domain.ProgressMonitors
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces;

    /// <summary>
    /// The progress monitor.
    /// </summary>
    public class ProgressMonitor : IProgressMonitor, IDisposable
    {
        /// <summary>
        /// The lock.
        /// </summary>
        private static readonly object Lock = new object();

        /// <summary>
        /// The id.
        /// </summary>
        private static int id = 0;

        /// <summary>
        /// The progress monitor items.
        /// </summary>
        private readonly ConcurrentDictionary<int, ProgressMonitorItem> progressMonitorItems =
            new ConcurrentDictionary<int, ProgressMonitorItem>();

        /// <summary>
        /// The background task.
        /// </summary>
        private readonly Task backgroundTask;

        /// <summary>
        /// The cancellation token source.
        /// </summary>
        private readonly CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// The progress logger.
        /// </summary>
        private readonly IProgressLogger progressLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressMonitor"/> class.
        /// </summary>
        /// <param name="progressLogger">
        /// The progress logger.
        /// </param>
        public ProgressMonitor(IProgressLogger progressLogger)
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            var token = this.cancellationTokenSource.Token;

            this.progressLogger = progressLogger;

            if (this.progressLogger != null)
            {
                this.backgroundTask = Task.Run(() => this.RunLogProgressTask(token), token);
            }
        }

        /// <summary>
        /// Gets or sets the job history update action.
        /// </summary>
        public Action JobHistoryUpdateAction { get; set; }

        /// <summary>
        /// The log progress.
        /// </summary>
        /// <param name="token">
        /// The token.
        /// </param>
        public void RunLogProgressTask(CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                this.LogProgress();

                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// The set progress item.
        /// </summary>
        /// <param name="progressMonitorItem">
        /// The progress monitor item.
        /// </param>
        public void SetProgressItem(ProgressMonitorItem progressMonitorItem)
        {

            //lock (Lock)
            {
                Interlocked.Increment(ref id);
                var key = progressMonitorItem.StepNumber;
                this.progressMonitorItems.AddOrUpdate(key, progressMonitorItem, (a, b) => progressMonitorItem);
                this.JobHistoryUpdateAction?.Invoke();
            }

        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.cancellationTokenSource?.Cancel();
            this.LogProgress();
        }

        /// <summary>
        /// The get snapshot of progress items.
        /// </summary>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        public IList<ProgressMonitorItem> GetSnapshotOfProgressItems()
        {
            return this.progressMonitorItems.ToArray().Select(a => a.Value).ToList();
        }

        /// <summary>
        /// The log progress.
        /// </summary>
        private void LogProgress()
        {
            this.progressLogger.Reset();

            //Process proc = Process.GetCurrentProcess();
            //Console.WriteLine($"Memory: {(proc.WorkingSet64 / 1024):N0} kb");

            var progressMonitorItems1 =
                this.progressMonitorItems.OrderBy(p => p.Key).Select(p => new { p.Key, p.Value }).ToList();

            this.progressLogger.LogHeader();

            foreach (var progressMonitorItem in progressMonitorItems1)
            {
                this.progressLogger.LogProgressMonitorItem(progressMonitorItem.Key, progressMonitorItem.Value);
            }
        }
    }
}