// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SanityTests.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SanityTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PipelineRunnerTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Client;
    using Fabric.Databus.Config;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Unity;

    /// <summary>
    /// The sanity tests.
    /// </summary>
    [TestClass]
    public class SanityTests
    {
        /// <summary>
        /// The test simple run.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [TestMethod]
        [Ignore]
        public async Task TestSimpleRun()
        {
            var localSaveFolder = Path.Combine(Path.GetTempPath(), "databus");
            Directory.Delete(localSaveFolder, true);
            Directory.CreateDirectory(localSaveFolder);

            var config = new QueryConfig
                             {
                                 ConnectionString = "server=(local);initial catalog=SharedDeId;Trusted_Connection=True;",
                                 Url = "https://HC2260.hqcatalyst.local/DataProcessingService/v1/BatchExecutions",
                                 MaximumEntitiesToLoad = 1000,
                                 EntitiesPerBatch = 100,
                                 EntitiesPerUploadFile = 100,
                                 LocalSaveFolder = localSaveFolder,
                                 DropAndReloadIndex = false,
                                 WriteTemporaryFilesToDisk = true,
                                 WriteDetailedTemporaryFilesToDisk = true,
                                 CompressFiles = false,
                                 UploadToUrl = false,
                                 Index = "Patients2",
                                 Alias = "patients",
                                 EntityType = "patient",
                                 UseMultipleThreads = false,
                                 KeepTemporaryLookupColumnsInOutput = true
                             };
            var jobData = new JobData
                              {
                                  DataModel = "{}",
                                  MyTopLevelDataSource = new TopLevelDataSource
                                                             {
                                                                 Key = "EDWPatientID",
                                                                 Sql =
                                                                     "SELECT 3 [EDWPatientID], 2 [BatchDefinitionId], 'Queued' [Status], 'Batch' [PipelineType]"
                                                             }
                              };

            var job = new Job { Config = config, Data = jobData };
            var runner = new DatabusRunner();
            await runner.RunRestApiPipelineAsync(new UnityContainer(), job, new CancellationToken());

            Console.WriteLine("Contents of folder after");

            Console.WriteLine("Directories");
            var directories = Directory.EnumerateDirectories(localSaveFolder);
            foreach (var directory in directories)
            {
                Console.WriteLine(directory);
            }

            Console.WriteLine("Files");
            var files = Directory.GetFiles(localSaveFolder);
            foreach (var file in files)
            {
                Console.WriteLine(file);
            }
        }
    }
}
