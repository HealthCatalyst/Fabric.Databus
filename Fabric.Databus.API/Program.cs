// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="">
//   
// </copyright>
// <summary>
//   The program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.API
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;

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
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static async Task<int> Main(string[] args)
        {
            Console.WriteLine("Current folder: " + Directory.GetCurrentDirectory());

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hosting.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var host = new WebHostBuilder()
                    .UseKestrel()
                    .UseUrls("http://*:5000") // https://georgestocker.com/2017/01/31/fix-for-asp-net-core-docker-service-not-being-exposed-on-host/
                    .UseConfiguration(config)
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .Build();

            Console.WriteLine("Databus is ready");

            var cts = new CancellationTokenSource();

            // handle the SIGTERM signal that docker sends: https://stackoverflow.com/questions/38291567/killing-gracefully-a-net-core-daemon-running-on-linux/47474693#47474693
            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
                {
                    Console.WriteLine("Received a ProcessExit command!");
                    cts.Cancel();
                };

            await host.RunAsync(cts.Token);

            return 0;
        }
    }
}
