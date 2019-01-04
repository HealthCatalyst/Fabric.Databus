// --------------------------------------------------------------------------------------------------------------------
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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Client;
    using Fabric.Databus.Config;
    using Fabric.Databus.Domain.ProgressMonitors;
    using Fabric.Databus.Interfaces.Loggers;
    using Fabric.Databus.Shared.Loggers;

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
        /// <exception cref="Exception">
        /// exception thrown
        /// </exception>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static async Task Main(string[] args)
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

                using (ProgressMonitor progressMonitor = new ProgressMonitor(new ConsoleProgressLogger()))
                {
                    using (var cancellationTokenSource = new CancellationTokenSource())
                    {
                        var container = new UnityContainer();
                        container.RegisterInstance<IProgressMonitor>(progressMonitor);

                        var pipelineRunner = new DatabusRunner();

                        await pipelineRunner.RunRestApiPipelineAsync(container, config, cancellationTokenSource.Token);
                    }
                }

                stopwatch.Stop();
                var timeElapsed = stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
                var threadText = config.Config.UseMultipleThreads ? "multiple threads" : "single thread";
                Console.WriteLine($"Finished in {timeElapsed} using {threadText}");

#if TRUE
                Serilog.Log.CloseAndFlush();

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
