using System;
using Serilog;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Fabric.Databus.API.Middleware
{
    public class GlobalErrorLoggingMiddleware
    {
        public static AppFunc Inject(AppFunc next, ILogger logger)
        {
            return async env =>
            {
                try
                {
                    await next(env);
                }
                catch (Exception ex)
                {
                    var contextSpecificLogger = logger.ForContext<GlobalErrorLoggingMiddleware>();
                    contextSpecificLogger.Error(ex, "Unhandled exception");
                    Console.WriteLine(ex.Message);
                }
            };
        }
    }
}
