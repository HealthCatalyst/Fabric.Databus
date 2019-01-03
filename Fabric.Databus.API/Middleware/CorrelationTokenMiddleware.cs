// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CorrelationTokenMiddleware.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the CorrelationTokenMiddleware type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.API.Middleware
{
    using System;
    using LibOwin;
    using Serilog.Context;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    /// <summary>
    /// The correlation token middleware.
    /// </summary>
    public class CorrelationTokenMiddleware
    {
        /// <summary>
        /// The correlation token header name.
        /// </summary>
        public static string CorrelationTokenHeaderName = "Correlation-Token";

        /// <summary>
        /// The correlation token context name.
        /// </summary>
        public static string CorrelationTokenContextName = "CorrelationToken";

        /// <summary>
        /// The inject.
        /// </summary>
        /// <param name="next">
        /// The next.
        /// </param>
        /// <returns>
        /// The <see cref="Func"/>.
        /// </returns>
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
