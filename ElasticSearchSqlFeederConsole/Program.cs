// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the Program type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Console
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using Fabric.Databus.Client;
    using Fabric.Databus.Config;
    using Fabric.Databus.Domain.ProgressMonitors;
    using Fabric.Databus.ElasticSearch;
    using Fabric.Databus.Http;
    using Fabric.Databus.Interfaces.ElasticSearch;
    using Fabric.Databus.Interfaces.Http;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.JsonSchema;
    using Fabric.Databus.Shared;
    using Fabric.Databus.Shared.Loggers;
    using Fabric.Shared;

    using PipelineRunner;

    using Serilog;

    using Unity;

    /// <summary>
    /// The program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <exception cref="Exception">exception thrown
        /// </exception>
        public static void Main(string[] args)
        {
            if (args.Any() && args[0] == "-generateschema")
            {
                if (args.Length < 2 || string.IsNullOrEmpty(args[1]))
                {
                    throw new Exception("You must specify a valid filename to write the schema to.");
                }

                var filename = args[1];

                JsonSchemaGenerator.WriteSchemaToFile(typeof(QueryConfig), filename);

                Console.WriteLine($"Written schema to {filename}");
                return;
            }

            try
            {
                string inputFile;
                if (!args.Any())
                {
                    do
                    {
                        Console.Write("Enter path to xml file: ");
                        inputFile = Console.ReadLine();
                    }
                    while (!File.Exists(inputFile));
                }
                else
                {
                    inputFile = args[0];
                }

                if (!File.Exists(inputFile))
                {
                    throw new Exception($"input file {inputFile} does not exist");
                }

                var config = new ConfigReader().ReadXml(inputFile);

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                ILogger logger = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo
                    .File(Path.Combine(Path.GetTempPath(), "Databus.out.txt")).CreateLogger();

                using (ProgressMonitor progressMonitor = new ProgressMonitor(new ConsoleProgressLogger()))
                {
                    using (var cancellationTokenSource = new CancellationTokenSource())
                    {
                        var container = new UnityContainer();
                        container.RegisterInstance<IProgressMonitor>(progressMonitor);

                        container.RegisterInstance(logger);

                        var pipelineRunner = new DatabusRunner();

                        if (config.Config.UploadToElasticSearch)
                        {
                            pipelineRunner.RunElasticSearchPipeline(container, config, cancellationTokenSource.Token);
                        }
                        else
                        {
                            pipelineRunner.RunRestApiPipeline(container, config, cancellationTokenSource.Token);
                        }
                    }
                }

                stopwatch.Stop();

                GC.Collect();

                var timeElapsed = stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
                var threadText = config.Config.UseMultipleThreads ? "multiple threads" : "single thread";
                var memoryInBytes = GC.GetTotalMemory(true);
                Console.WriteLine($"Finished in {timeElapsed} using {threadText}, Memory Working set: {memoryInBytes.ToDisplayString()}");

#if TRUE
                logger.Verbose("Finished in {ElapsedMinutes} minutes on {Date}.", stopwatch.Elapsed.TotalMinutes, DateTime.Today);
                //logger.Error(new Exception("test"), "An error has occurred.");

                Log.CloseAndFlush();

                //file.Flush();
                //file.Close();
                //file.Dispose();
                //file = null;
#endif
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("(Type any key to exit)");
            Console.ReadKey();
        }

        /// <summary>
        /// The run databus.
        /// </summary>
        public static void RunDatabus()
        {
            var config = new QueryConfig
            {
                ConnectionString = "server=(local);initial catalog=SharedDeId;Trusted_Connection=True;",
                Url = "https://HC2260.hqcatalyst.local/DataProcessingService/v1/BatchExecutions",
                MaximumEntitiesToLoad = 1000,
                EntitiesPerBatch = 100,
                EntitiesPerUploadFile = 100,
                LocalSaveFolder = @"C:\Catalyst\databus",
                DropAndReloadIndex = false,
                WriteTemporaryFilesToDisk = true,
                WriteDetailedTemporaryFilesToDisk = true,
                CompressFiles = false,
                UploadToElasticSearch = false,
                Index = "Patients2",
                Alias = "patients",
                EntityType = "patient",
                TopLevelKeyColumn = "EDWPatientID",
                UseMultipleThreads = false,
                KeepTemporaryLookupColumnsInOutput = true
            };
            var jobData = new JobData
            {
                DataModel = "{}",
                MyDataSources = new List<DataSource>
                                                      {
                                                          new DataSource
                                                              {
                                                                  Sql =
                                                                      "SELECT 3 [EDWPatientID], 2 [BatchDefinitionId], 'Queued' [Status], 'Batch' [PipelineType]"
                                                              }
                                                      }
            };
            var job = new Job { Config = config, Data = jobData };
            var runner = new DatabusRunner();
            runner.RunRestApiPipeline(new UnityContainer(), job, new CancellationToken());
        }
    }
}
