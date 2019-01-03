// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValidateJobModule.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ValidateJobModule type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.API.Modules
{
    using System;
    using System.Linq;

    using Fabric.Databus.API.Configuration;
    using Fabric.Databus.Domain.Jobs;

    using Nancy;
    using Nancy.Extensions;
    using Nancy.IO;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The validate job module.
    /// </summary>
    public sealed class ValidateJobModule : NancyModule
    {
        /// <inheritdoc />
        public ValidateJobModule(ILogger logger, IJobScheduler jobScheduler, IAppConfiguration configuration) : base("/validate")
        {
            this.RequiresClaimsIfAuthorizationEnabled(configuration, claim => claim.Value.Equals("fabric/databus.validate", StringComparison.OrdinalIgnoreCase));

            this.Post(
                "/",
                async parameters =>
                    {
                        var jobName = string.Empty; // parameters.jobName;

                var httpFiles = this.Request.Files.ToList();

                var body = RequestStream.FromStream(this.Request.Body).AsString();
                return await jobScheduler.ValidateJobAsync(body, jobName, logger);
            });
        }
    }
}
