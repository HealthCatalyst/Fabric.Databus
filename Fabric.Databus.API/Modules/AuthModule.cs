// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthModule.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the AuthModule type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.API.Modules
{
    using System;
    using System.Linq;

    using Nancy;

    /// <inheritdoc />
    /// <summary>
    /// The authorization module.
    /// </summary>
    public sealed class AuthModule : NancyModule
    {
        /// <inheritdoc />
        public AuthModule() : base("/auth")
        {
            this.Post(
                "/",
                parameters =>
                    {
                        var canRunJobs = this.Context.CurrentUser.Claims.Any(c => c.Value.Equals("fabric/databus.queuejob", StringComparison.OrdinalIgnoreCase));
                var canValidate = this.Context.CurrentUser.Claims.Any(c => c.Value.Equals("fabric/databus.validate", StringComparison.OrdinalIgnoreCase));

                return new { CanRunJobs = canRunJobs, CanValidate = canValidate, User = this.Context.CurrentUser };
            });
        }
    }
}
