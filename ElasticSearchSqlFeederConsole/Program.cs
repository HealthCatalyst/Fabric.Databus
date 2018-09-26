﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="">
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
    using System.Linq;
    using System.Threading;

    using ElasticSearchSqlFeeder.Shared;

    using Fabric.Databus.Config;
    using Fabric.Databus.Domain.ProgressMonitors;

    using PipelineRunner;
    using Serilog;

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
        /// <exception cref="Exception">exception
        /// </exception>
        public static void Main(string[] args)
        {
            if (!args.Any()) throw new Exception("Please pass the job.xml file as a parameter");

            if (args[0] == "-generateschema")
            {
                if (args.Length < 2 || string.IsNullOrEmpty(args[1])) throw new Exception("You must specify a valid filename to write the schema to.");

                var filename = args[1];

                JsonSchemaValidator.JsonSchemaGenerator.WriteSchemaToFile(typeof(QueryConfig), 
                    filename);

                Console.WriteLine($"Written schema to {filename}");
                return;
            }

            string inputFile = args[0];

            var config = new ConfigReader().ReadXml(inputFile);

#if TRUE
//            Serilog.Debugging.SelfLog.Enable(Console.Error);

            //var file = File.CreateText(@"c:\temp\serilog.out");
            //Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(file));

            var logger = new LoggerConfiguration()
              .ReadFrom.AppSettings()
              .CreateLogger();

#endif

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            using (ProgressMonitor progressMonitor = new ProgressMonitor(new ConsoleProgressLogger()))
            {
                using (var cancellationTokenSource = new CancellationTokenSource())
                {
                    new PipelineRunner().RunPipeline(config, progressMonitor, cancellationTokenSource.Token);
                }
            }

            stopwatch.Stop();

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
