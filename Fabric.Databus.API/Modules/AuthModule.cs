using System;
using System.Linq;
using Nancy;

namespace Fabric.Databus.API.Modules
{
		public class AuthModule : NancyModule
		{
				public AuthModule() : base("/auth")
				{
						Post("/", parameters =>
						{
								var canRunJobs = Context.CurrentUser.Claims.Any(c => c.Value.Equals("fabric/databus.queuejob", StringComparison.OrdinalIgnoreCase));
								var canValidate = Context.CurrentUser.Claims.Any(c => c.Value.Equals("fabric/databus.validate", StringComparison.OrdinalIgnoreCase));

								return new { CanRunJobs = canRunJobs, CanValidate = canValidate, User = Context.CurrentUser };
						});
				}
		}
}
