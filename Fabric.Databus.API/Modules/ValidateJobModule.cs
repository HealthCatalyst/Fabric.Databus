using System;
using System.Linq;
using Fabric.Databus.Domain.Jobs;
using Nancy;
using Nancy.Extensions;
using Nancy.IO;
using Nancy.Security;
using Serilog;

namespace Fabric.Databus.API.Modules
{
		public class ValidateJobModule : NancyModule
		{
				public ValidateJobModule(ILogger logger, IJobScheduler jobScheduler) : base("/validate")
				{
						// this.RequiresClaims(claim => claim.Value.Equals("fabric/databus.validate", StringComparison.OrdinalIgnoreCase));

						Post("/", async parameters =>
						{
								var jobName = string.Empty; // parameters.jobName;

								var httpFiles = Request.Files.ToList();

								var body = RequestStream.FromStream(Request.Body).AsString();
								return await jobScheduler.ValidateJob(body, jobName);
						});
				}
		}
}
