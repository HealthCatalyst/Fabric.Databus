// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PerformanceLoggingMiddleware.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the PerformanceLoggingMiddleware type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.API.Middleware
{
    using System;
    using System.Diagnostics;
    using LibOwin;
    using Serilog;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    /// <summary>
    /// The performance logging middleware.
    /// </summary>
    public class PerformanceLoggingMiddleware
    {
        /// <summary>
        /// The inject.
        /// </summary>
        /// <param name="next">
        /// The next.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <returns>
        /// The <see cref="Func"/>.
        /// </returns>
        public static AppFunc Inject(AppFunc next, ILogger logger)
        {
            return async env =>
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                await next(env);
                stopWatch.Stop();
                var owinContext = new OwinContext(env);
                var contextSpecificLogger = logger.ForContext<PerformanceLoggingMiddleware>();
                contextSpecificLogger.Information(
                    "Request: {@Method} {@Path} executed in {@RequestTime:000} ms",
                    owinContext.Request.Method,
                    owinContext.Request.Path,
                    stopWatch.ElapsedMilliseconds);
            };
        }
    }
}
