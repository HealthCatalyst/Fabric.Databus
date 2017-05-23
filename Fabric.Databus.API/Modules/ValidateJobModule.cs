using System.IO;
using System.Linq;
using Fabric.Databus.Domain.Jobs;
using Nancy;
using Nancy.Extensions;
using Nancy.IO;
using Nancy.ModelBinding;
using Serilog;
using SqlImporter;

namespace Fabric.Databus.API.Modules
{
    public class ValidateJobModule : NancyModule
    {
        public ValidateJobModule(ILogger logger, IJobScheduler jobScheduler) : base("/validate")
        {
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
