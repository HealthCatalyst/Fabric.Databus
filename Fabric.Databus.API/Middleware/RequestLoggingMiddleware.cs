using System;
using System.IO;
using System.Text;
using LibOwin;
using Serilog;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Fabric.Databus.API.Middleware
{
    public class RequestLoggingMiddleware
    {
        public static AppFunc Inject(AppFunc next, ILogger logger)
        {
            return async env =>
            {
                var contextSpecificLogger = logger.ForContext<RequestLoggingMiddleware>();
                var owinContext = new OwinContext(env);
                //contextSpecificLogger.Information("Test");

                // below causes body to be already read

                //string body;
                //// read from stream without disposing the stream
                //using (var reader = new StreamReader(owinContext.Request.Body, Encoding.UTF8, true, 1024, true))
                //{
                //    body = reader.ReadToEnd();
                //}

                //contextSpecificLogger.Information("Incoming request: {@Method}, {@Path}, {@Headers}, {@Body}",
                //        owinContext.Request.Method,
                //        owinContext.Request.Path,
                //        owinContext.Request.Headers,
                //        body);

                Console.WriteLine($"Incoming request: {owinContext.Request.Method} {owinContext.Request.Path}");

                await next(env);

                //contextSpecificLogger.Information("Outgoing response: {@StatusCode}, {@Headers}, {@Body}",
                //    owinContext.Response.StatusCode,
                //    owinContext.Response.Headers,
                //    owinContext.Response.Body);
            };
        }
    }
}
