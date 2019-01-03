// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JobStatusModule.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the JobStatusModule type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.API.Modules
{
    using System;
    using System.Linq;

    using Fabric.Databus.API.Configuration;
    using Fabric.Databus.Domain.Jobs;
    using Fabric.Databus.Shared;

    using Nancy;
    using Nancy.Security;

    using Serilog;

    /// <summary>
    /// The job status module.
    /// </summary>
    public sealed class JobStatusModule : NancyModule
    {
        /// <inheritdoc />
        public JobStatusModule(ILogger logger, IJobScheduler jobScheduler, IAppConfiguration configuration) : base("/jobstatus")
        {
            this.RequiresClaimsIfAuthorizationEnabled(configuration, claim => claim.Value.Equals("fabric/databus.queuejob", StringComparison.OrdinalIgnoreCase));

            this.Get(
                "/",
                parameters =>
                    {
                        var lastjob = jobScheduler.GetMostRecentJobs(1).FirstOrDefault();

                        return this.Negotiate.WithModel(lastjob).WithView("ShowJobStatus");
                    });

            this.Get(
                "/{jobName}",
                parameters =>
                    {
                        var jobName = parameters.jobName;
                if (Guid.TryParse(jobName, out Guid jobGuid))
                {
                    var model = jobScheduler.GetJobStatus(jobGuid);

                    return this.Negotiate.WithModel(model).WithView("ShowJobStatus");
                }

                return jobScheduler.GetJobHistory(parameters.jobName);
            });
        }
    }
}
