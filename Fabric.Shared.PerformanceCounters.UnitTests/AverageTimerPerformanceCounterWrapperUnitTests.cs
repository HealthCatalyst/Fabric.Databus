// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnitTest1.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the AverageTimerPerformanceCounterWrapperUnitTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Shared.PerformanceCounters.UnitTests
{
    using System.Threading;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// The average timer performance counter wrapper unit tests.
    /// </summary>
    [TestClass]
    public class AverageTimerPerformanceCounterWrapperUnitTests
    {
        /// <summary>
        /// The can store performance counter successfully.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void CanStorePerformanceCounterSuccessfully()
        {
            for (int i = 0; i < 100; i++)
            {
                using (var counter = new AverageTimerPerformanceCounterWrapper(
                "Fabric.Databus",
                "BatchAverageTime",
                "BaseBatchAverageTime"))
                {
                    int millisecondsTimeout = 100 + (i * 10);
                    Thread.Sleep(millisecondsTimeout);
                }
            }
        }
    }
}
