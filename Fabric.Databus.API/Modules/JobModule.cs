// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JobModule.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the JobModule type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.API.Modules
{
    using System;

    using Fabric.Databus.API.Configuration;
    using Fabric.Databus.Config;
    using Fabric.Databus.Domain.Jobs;
    using Fabric.Shared;

    using Nancy;
    using Nancy.Extensions;
    using Nancy.IO;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The job module.
    /// </summary>
    public sealed class JobModule : NancyModule
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Fabric.Databus.API.Modules.JobModule" /> class.
        /// </summary>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="jobScheduler">
        /// The job scheduler.
        /// </param>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        public JobModule(ILogger logger, IJobScheduler jobScheduler, IAppConfiguration configuration) : base("/jobs")
        {
            this.RequiresClaimsIfAuthorizationEnabled(configuration, claim => claim.Value.Equals("fabric/databus.queuejob", StringComparison.OrdinalIgnoreCase));

            this.Post(
                "/",
                parameters =>
                    {
                        var body = RequestStream.FromStream(Request.Body).AsString();
                var queryConfig = body.FromXml<Job>();

                var jobId = jobScheduler.ExecuteJobImmediately(queryConfig);

                var uriBuilder = new UriBuilder(
                    this.Request.Url.Scheme,
                    this.Request.Url.HostName,
                    this.Request.Url.Port ?? 80,
                    $"jobstatus/{jobId}");

                var statusUri = uriBuilder.ToString();

                var model = new { JobId = jobId, links = new[] { new { status = statusUri } } };

                return this.Negotiate.WithModel(model).WithStatusCode(HttpStatusCode.Accepted)
                    .WithHeader("Location", statusUri);
                    });

        }
    }
}
