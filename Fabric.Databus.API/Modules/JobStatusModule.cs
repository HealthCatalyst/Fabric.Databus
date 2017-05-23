using System;
using ElasticSearchSqlFeeder.Shared;
using Fabric.Databus.Domain.Jobs;
using Nancy;
using Nancy.ModelBinding;
using Serilog;
using SqlImporter;

namespace Fabric.Databus.API.Modules
{
    public class JobStatusModule : NancyModule
    {
        public JobStatusModule(ILogger logger, IJobScheduler jobScheduler) : base("/jobstatus")
        {
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
