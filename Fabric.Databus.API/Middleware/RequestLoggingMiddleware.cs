// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequestLoggingMiddleware.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the RequestLoggingMiddleware type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.API.Middleware
{
    using System;
    using System.IO;
    using System.Text;
    using LibOwin;
    using Serilog;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    /// <summary>
    /// The request logging middleware.
    /// </summary>
    public class RequestLoggingMiddleware
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
                var contextSpecificLogger = logger.ForContext<RequestLoggingMiddleware>();
                var owinContext = new OwinContext(env);
                //// contextSpecificLogger.Information("Test");

                // below causes body to be already read

                // string body;
                //// read from stream without disposing the stream
                // using (var reader = new StreamReader(owinContext.Request.Body, Encoding.UTF8, true, 1024, true))
                // {
                //    body = reader.ReadToEnd();
                // }

                // contextSpecificLogger.Information("Incoming request: {@Method}, {@Path}, {@Headers}, {@Body}",
                //        owinContext.Request.Method,
                //        owinContext.Request.Path,
                //        owinContext.Request.Headers,
                ////        body);

                Console.WriteLine($"Incoming request: {owinContext.Request.Method} {owinContext.Request.Path}");

                await next(env);

                // contextSpecificLogger.Information("Outgoing response: {@StatusCode}, {@Headers}, {@Body}",
                //    owinContext.ResponseContent.StatusCode,
                //    owinContext.ResponseContent.Headers,
                //    owinContext.ResponseContent.Body);
            };
        }
    }
}
