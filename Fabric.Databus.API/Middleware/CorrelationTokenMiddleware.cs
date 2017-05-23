using System;
using LibOwin;
using Serilog.Context;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Fabric.Databus.API.Middleware
{
    public class CorrelationTokenMiddleware
    {
        public static string CorrelationTokenHeaderName = "Correlation-Token";
        public static string CorrelationTokenContextName = "CorrelationToken";
        public static AppFunc Inject(AppFunc next)
        {
            return async env =>
            {
                var owinContext = new OwinContext(env);
                var existingCorrelationToken = owinContext.Request.Headers[CorrelationTokenHeaderName];
                if (!Guid.TryParse(existingCorrelationToken, out Guid correlationToken))
                {
                    correlationToken = Guid.NewGuid();
                }
                owinContext.Set(CorrelationTokenContextName, correlationToken.ToString());
                using (LogContext.PushProperty(CorrelationTokenContextName, correlationToken))
                {
                    await next(env);
                }
            };
        }
    }
}
