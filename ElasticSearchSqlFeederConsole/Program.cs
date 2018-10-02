﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the Program type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ElasticSearchSqlFeederConsole
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using Fabric.Databus.Config;
    using Fabric.Databus.Domain.ProgressMonitors;
    using Fabric.Databus.ElasticSearch;
    using Fabric.Databus.Interfaces;
    using Fabric.Databus.JsonSchema;
    using Fabric.Databus.Shared;

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
            if (!args.Any())
            {
                throw new Exception("Please pass the job.xml file as a parameter");
            }

            if (args[0] == "-generateschema")
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

            string inputFile = args[0];

            var config = new ConfigReader().ReadXml(inputFile);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            ILogger logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(Path.Combine(Path.GetTempPath(), "Databus.out.txt"))
                .CreateLogger();

            using (ProgressMonitor progressMonitor = new ProgressMonitor(new ConsoleProgressLogger()))
            {
                using (var cancellationTokenSource = new CancellationTokenSource())
                {
                    var container = new UnityContainer();
                    var databusSqlReader = new DatabusSqlReader(config.Config.ConnectionString, 0);
                    container.RegisterInstance<IDatabusSqlReader>(databusSqlReader);
                    container.RegisterType<IElasticSearchUploaderFactory, ElasticSearchUploaderFactory>();
                    container.RegisterType<IElasticSearchUploader, ElasticSearchUploader>();
                    container.RegisterType<IHttpClientFactory, HttpClientFactory>();
                    container.RegisterInstance(logger);

                    if (config.Config.UseMultipleThreads)
                    {
                        container.RegisterType<IPipelineExecutorFactory, MultiThreadedPipelineExecutorFactory>();
                    }
                    else
                    {
                        container.RegisterType<IPipelineExecutorFactory, SingleThreadedPipelineExecutorFactory>();
                    }

                    var pipelineRunner = new PipelineRunner(container, cancellationTokenSource.Token);

                    pipelineRunner.RunPipeline(config, progressMonitor);
                }
            }

            stopwatch.Stop();
            var timeElapsed = stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
            var threadText = config.Config.UseMultipleThreads ? "multiple threads" : "single thread";
            Console.WriteLine($"Finished in {timeElapsed} using {threadText}");

#if TRUE
            logger.Verbose("Finished in {ElapsedMinutes} minutes on {Date}.", stopwatch.Elapsed.TotalMinutes, DateTime.Today);
            //logger.Error(new Exception("test"), "An error has occurred.");

            Log.CloseAndFlush();

            //file.Flush();
            //file.Close();
            //file.Dispose();
            //file = null;
#endif
            Console.WriteLine("(Type any key to exit)");
            Console.ReadKey();
        }

    }
}
