// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlobalErrorLoggingMiddleware.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the GlobalErrorLoggingMiddleware type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.API.Middleware
{
    using System;
    using Serilog;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    /// <summary>
    /// The global error logging middleware.
    /// </summary>
    public class GlobalErrorLoggingMiddleware
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
