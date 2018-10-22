using System;
using ElasticSearchSqlFeeder.Shared;
using Fabric.Databus.Domain.Jobs;
using Nancy;
using Nancy.Security;
using Serilog;

namespace Fabric.Databus.API.Modules
{
		public class JobStatusModule : NancyModule
		{
				public JobStatusModule(ILogger logger, IJobScheduler jobScheduler) : base("/jobstatus")
				{
						// this.RequiresClaims(claim => claim.Value.Equals("fabric/databus.queuejob", StringComparison.OrdinalIgnoreCase));

						Get("/", parameters => jobScheduler.GetMostRecentJobs(10));

						Get("/{jobName}", parameters =>
						{
								var jobName = parameters.jobName;
								if (Guid.TryParse(jobName, out Guid jobGuid))
								{
										JobHistoryItem model = jobScheduler.GetJobStatus(jobGuid);

										return Negotiate
												.WithModel(model)
												.WithView("ShowJobStatus");
								}

								return jobScheduler.GetJobHistory(parameters.jobName);
						});
				}
		}
}
