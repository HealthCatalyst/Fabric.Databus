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

    /// <summary>
    /// The pipeline step state.
    /// </summary>
    public class PipelineStepState
    {
        /// <summary>
        /// The processing time by query id.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public ConcurrentDictionary<string, TimeSpan> ProcessingTimeByQueryId = new ConcurrentDictionary<string, TimeSpan>();

        /// <summary>
        /// The total items added to output queue by query id.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public ConcurrentDictionary<string, int> TotalItemsAddedToOutputQueueByQueryId = new ConcurrentDictionary<string, int>();

        /// <summary>
        /// The _total items processed.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public int totalItemsProcessed;

        /// <summary>
        /// The _total items added to output queue.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public int totalItemsAddedToOutputQueue;

        /// <summary>
        /// The _processing time.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public TimeSpan processingTime = TimeSpan.Zero;

        /// <summary>
        /// The _blocked time.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public TimeSpan blockedTime = TimeSpan.Zero;

        /// <summary>
        /// The processor count.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public int currentInstancesOfStep;

        /// <summary>
        /// The max processor count.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public int maximumInstancesOfStep;

        /// <summary>
        /// The error text.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public string errorText;

        public int numberOfEntitiesUploadedForBatch;

        public int numberOfEntitiesUploadedForJob;

        public int currentBatchFileNumber;
    }
}
