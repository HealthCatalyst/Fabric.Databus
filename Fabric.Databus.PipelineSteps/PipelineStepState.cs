// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PipelineStepStatus.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the PipelineStepStatus type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.PipelineSteps
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    /// <summary>
    /// The pipeline step state.
    /// </summary>
    public class PipelineStepState
    {
        /// <summary>
        /// The _total items processed.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private int totalItemsProcessed;

        /// <summary>
        /// The _total items added to output queue.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private int totalItemsAddedToOutputQueue;

        /// <summary>
        /// The processor count.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private int currentInstancesOfStep;

        /// <summary>
        /// The max processor count.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private int maximumInstancesOfStep;

        /// <summary>
        /// The number of entities uploaded for batch.
        /// </summary>
        private int numberOfEntitiesUploadedForBatch;

        /// <summary>
        /// The number of entities uploaded for job.
        /// </summary>
        private int numberOfEntitiesUploadedForJob;

        /// <summary>
        /// The current batch file number.
        /// </summary>
        private int currentBatchFileNumber;

        /// <summary>
        /// Gets or sets the processing time.
        /// </summary>
        public TimeSpan ProcessingTime { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Gets or sets the blocked time.
        /// </summary>
        public TimeSpan BlockedTime { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// The total items processed.
        /// </summary>
        public int TotalItemsProcessed => this.totalItemsProcessed;

        /// <summary>
        /// The _total items added to output queue.
        /// </summary>
        public int TotalItemsAddedToOutputQueue => this.totalItemsAddedToOutputQueue;

        /// <summary>
        /// The processor count.
        /// </summary>
        public int CurrentInstancesOfStep => this.currentInstancesOfStep;

        /// <summary>
        /// The max processor count.
        /// </summary>
        public int MaximumInstancesOfStep => this.maximumInstancesOfStep;

        /// <summary>
        /// Gets the processing time by query id.
        /// </summary>
        public ConcurrentDictionary<string, TimeSpan> ProcessingTimeByQueryId { get; } = new ConcurrentDictionary<string, TimeSpan>();

        /// <summary>
        /// Gets the total items added to output queue by query id.
        /// </summary>
        public ConcurrentDictionary<string, int> TotalItemsAddedToOutputQueueByQueryId { get; } = new ConcurrentDictionary<string, int>();

        /// <summary>
        /// The number of entities uploaded for batch.
        /// </summary>
        public int NumberOfEntitiesUploadedForBatch => this.numberOfEntitiesUploadedForBatch;

        /// <summary>
        /// The number of entities uploaded for job.
        /// </summary>
        public int NumberOfEntitiesUploadedForJob => this.numberOfEntitiesUploadedForJob;

        /// <summary>
        /// The current batch file number.
        /// </summary>
        public int CurrentBatchFileNumber => this.currentBatchFileNumber;

        /// <summary>
        /// Gets or sets the error text.
        /// </summary>
        public string ErrorText { get; set; }

        /// <summary>
        /// The increment total items processed.
        /// </summary>
        public void IncrementTotalItemsProcessed()
        {
            Interlocked.Increment(ref this.totalItemsProcessed);
        }

        /// <summary>
        /// The increment total items added to output queue.
        /// </summary>
        public void IncrementTotalItemsAddedToOutputQueue()
        {
            Interlocked.Increment(ref this.totalItemsAddedToOutputQueue);
        }

        /// <summary>
        /// The increment current instances of step.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int IncrementCurrentInstancesOfStep()
        {
            return Interlocked.Increment(ref this.currentInstancesOfStep);
        }

        /// <summary>
        /// The increment maximum instances of step.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int IncrementMaximumInstancesOfStep()
        {
            return Interlocked.Increment(ref this.maximumInstancesOfStep);
        }

        /// <summary>
        /// The increment number of entities uploaded for batch.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int IncrementNumberOfEntitiesUploadedForBatch()
        {
            return Interlocked.Increment(ref this.numberOfEntitiesUploadedForBatch);
        }

        /// <summary>
        /// The reset number of entities uploaded for batch.
        /// </summary>
        public void ResetNumberOfEntitiesUploadedForBatch()
        {
            Interlocked.Exchange(ref this.numberOfEntitiesUploadedForBatch, 0);
        }

        /// <summary>
        /// The increment number of entities uploaded for job.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int IncrementNumberOfEntitiesUploadedForJob()
        {
            return Interlocked.Increment(ref this.numberOfEntitiesUploadedForJob);
        }

        /// <summary>
        /// The reset number of entities uploaded for job.
        /// </summary>
        public void ResetNumberOfEntitiesUploadedForJob()
        {
            Interlocked.Exchange(ref this.numberOfEntitiesUploadedForJob, 0);
        }

        /// <summary>
        /// The increment current batch file number.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int IncrementCurrentBatchFileNumber()
        {
            return Interlocked.Increment(ref this.currentBatchFileNumber);
        }
    }
}
