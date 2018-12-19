// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AverageTimerPerformanceCounterWrapper.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the AverageTimerPerformanceCounterWrapper type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Shared.PerformanceCounters
{
    using System;
    using System.Diagnostics;

    /// <inheritdoc />
    /// <summary>
    /// The performance counter wrapper.
    /// </summary>
    public class AverageTimerPerformanceCounterWrapper : IDisposable
    {
        /// <summary>
        /// The performance counter.
        /// </summary>
        private static PerformanceCounter performanceCounter;

        /// <summary>
        /// The base performance counter.
        /// </summary>
        private static PerformanceCounter basePerformanceCounter;

        /// <summary>
        /// The stop watch.
        /// </summary>
        private static Stopwatch stopWatch;

        /// <summary>
        /// Initializes a new instance of the <see cref="AverageTimerPerformanceCounterWrapper"/> class.
        /// </summary>
        /// <param name="categoryName">
        /// The category name.
        /// </param>
        /// <param name="counterName">
        /// The counter name.
        /// </param>
        /// <param name="baseCounterName">
        /// The base Counter Name.
        /// </param>
        public AverageTimerPerformanceCounterWrapper(string categoryName, string counterName, string baseCounterName)
        {
            if (performanceCounter == null)
            {
                RegisterCounters(categoryName, counterName, baseCounterName);
                CreateCounters(categoryName, counterName, baseCounterName);
                stopWatch = new Stopwatch();
            }

            stopWatch.Start();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            performanceCounter.IncrementBy(stopWatch.ElapsedTicks);
            basePerformanceCounter.Increment();
            stopWatch.Stop();
        }

        /// <summary>
        /// The register counters.
        /// </summary>
        /// <param name="categoryName">
        /// The category name.
        /// </param>
        /// <param name="counterName">
        /// counter Name
        /// </param>
        /// <param name="baseCounterName">
        /// The base Counter Name.
        /// </param>
        private static void RegisterCounters(string categoryName, string counterName, string baseCounterName)
        {
            if (!PerformanceCounterCategory.Exists(categoryName))
            {

                var counterDataCollection = new CounterCreationDataCollection();

                // Add the counter.
                var averageCount64 = new CounterCreationData
                                                         {
                                                             CounterType = PerformanceCounterType.AverageTimer32,
                                                             CounterName = counterName
                                                         };
                counterDataCollection.Add(averageCount64);

                // Add the base counter.
                var averageCount64Base = new CounterCreationData
                                                             {
                                                                 CounterType = PerformanceCounterType.AverageBase,
                                                                 CounterName = baseCounterName
                                                             };
                counterDataCollection.Add(averageCount64Base);

                // Create the category.
                PerformanceCounterCategory.Create(
                    categoryName,
                    "Demonstrates usage of the AverageCounter64 performance counter type.",
                    PerformanceCounterCategoryType.SingleInstance,
                    counterDataCollection);
            }
        }

        /// <summary>
        /// The create counters.
        /// </summary>
        /// <param name="categoryName">
        /// The category name.
        /// </param>
        /// <param name="counterName">
        /// The counter name.
        /// </param>
        /// <param name="baseCounterName">
        /// The base counter name.
        /// </param>
        private static void CreateCounters(string categoryName, string counterName, string baseCounterName)
        {
            // Create the counters.
            performanceCounter = new PerformanceCounter(categoryName, counterName, false);

            basePerformanceCounter = new PerformanceCounter(categoryName, baseCounterName, false);

            performanceCounter.RawValue = 0;
            basePerformanceCounter.RawValue = 0;
        }
    }
}
