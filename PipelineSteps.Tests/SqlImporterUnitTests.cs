// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlImporterUnitTests.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SqlImporterUnitTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PipelineStep.Tests
{
    using System;
    using System.IO;
    using System.Threading;

    using CreateBatchItemsPipelineStep;

    using Fabric.Databus.Config;
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.Shared.Queues;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Serilog;

    using SqlImportPipelineStep;

    /// <summary>
    /// The sql importer unit tests.
    /// </summary>
    [TestClass]
    public class SqlImporterUnitTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var job = new Job
            {
                Config = new QueryConfig
                {
                    LocalSaveFolder = Path.GetTempPath(),
                    EntitiesPerUploadFile = 1
                }
            };

            var queueManager = new QueueManager();

            var logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                //var createBatchItemsQueueProcessor = new SqlImportPipelineStep(
                //    job.Config,
                //    logger,
                //    queueManager,
                //    new MockProgressMonitor(),
                //    cancellationTokenSource.Token);

                //var stepNumber = 1;
                //queueManager.CreateInputQueue<IJsonObjectQueueItem>(stepNumber);

                //createBatchItemsQueueProcessor.CreateOutQueue(stepNumber);

                //createBatchItemsQueueProcessor.InitializeWithStepNumber(stepNumber);
            }
        }
    }
}
