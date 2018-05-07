using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Fabric.Databus.API.Configuration;
using Nancy;
using Nancy.Security;

namespace Fabric.Databus.API
{
    public static class ModuleSecurity
    {
        public static void RequiresClaimsIfAuthorizationEnabled(this NancyModule module, IAppConfiguration configuration, Predicate<Claim> claim)
        {
            if (configuration.EnableAuthorization)
            {
                module.RequiresClaims(claim);
            }
        }
    }
}
