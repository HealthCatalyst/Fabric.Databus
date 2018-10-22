using System;
using ElasticSearchSqlFeeder.Shared;
using Fabric.Databus.Domain.Jobs;
using Nancy;
using Nancy.Security;
using Serilog;

namespace Fabric.Databus.API.Modules
{
		public class TestModule : NancyModule
		{
				public TestModule(ILogger logger, IJobScheduler jobScheduler) : base("/test")
				{
						Get("/", parameters => new { Text = "hello" });
				}
		}
}
