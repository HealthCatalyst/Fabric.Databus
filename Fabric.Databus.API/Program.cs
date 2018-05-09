using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Fabric.Databus.API
{
    public class Program
    {
        public static void Main(string[] args)
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

            host.Run();
        }
    }
}
