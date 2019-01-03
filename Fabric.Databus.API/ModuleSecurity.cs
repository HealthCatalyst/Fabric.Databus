// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModuleSecurity.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ModuleSecurity type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.API
{
    using System;
    using System.Security.Claims;

    using Fabric.Databus.API.Configuration;

    using Nancy;
    using Nancy.Security;

    /// <summary>
    /// The module security.
    /// </summary>
    public static class ModuleSecurity
    {
        /// <summary>
        /// The requires claims if authorization enabled.
        /// </summary>
        /// <param name="module">
        /// The module.
        /// </param>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <param name="claim">
        /// The claim.
        /// </param>
        public static void RequiresClaimsIfAuthorizationEnabled(this NancyModule module, IAppConfiguration configuration, Predicate<Claim> claim)
        {
            if (configuration.EnableAuthorization)
            {
                module.RequiresClaims(claim);
            }
        }
    }
}
