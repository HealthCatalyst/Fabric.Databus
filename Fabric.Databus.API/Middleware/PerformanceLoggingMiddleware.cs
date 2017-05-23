using System;
using System.Diagnostics;
using LibOwin;
using Serilog;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Fabric.Databus.API.Middleware
{
    public class PerformanceLoggingMiddleware
    {
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
                contextSpecificLogger.Information("Request: {@Method} {@Path} executed in {@RequestTime:000} ms",
                    owinContext.Request.Method, owinContext.Request.Path, stopWatch.ElapsedMilliseconds);
            };
        }
    }
}
