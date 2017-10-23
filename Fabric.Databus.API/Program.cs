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
						var config = new ConfigurationBuilder()
							.SetBasePath(Directory.GetCurrentDirectory())
							.AddJsonFile("hosting.json", optional: true)
							.AddEnvironmentVariables()
							.Build();

						var host = new WebHostBuilder()
								.UseKestrel()
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
