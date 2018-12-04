﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the Program type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Nuget.Console
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using Fabric.Databus.Client;
    using Fabric.Databus.Config;
    using Fabric.Databus.Domain.ProgressMonitors;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Shared.Loggers;

    using Serilog;

    using Unity;

    using Console = System.Console;

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
            Console.WriteLine("Running Fabric.Databus.Nuget.Console");

            if (!args.Any())
            {
                throw new Exception("Please pass the job.xml file as a parameter");
            }

            try
            {
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("(Type any key to exit)");
            Console.ReadKey();
        }
    }
}
