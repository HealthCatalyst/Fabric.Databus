// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagnosticsMiddleware.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the DiagnosticsMiddleware type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.API.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using LibOwin;
    using Serilog.Core;
    using Serilog.Events;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    /// <summary>
    /// The diagnostics middleware.
    /// </summary>
    public class DiagnosticsMiddleware
    {
        /// <summary>
        /// The log level parameter.
        /// </summary>
        private const string LogLevelParameter = "LogLevel";

        /// <summary>
        /// The diagnostics path.
        /// </summary>
        private static readonly PathString DiagnosticsPath = new PathString("/_diagnostics");

        /// <summary>
        /// The _level switch.
        /// </summary>
        private readonly LoggingLevelSwitch levelSwitch;

        /// <summary>
        /// The next.
        /// </summary>
        private readonly AppFunc next;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticsMiddleware"/> class.
        /// </summary>
        /// <param name="next">
        /// The next.
        /// </param>
        /// <param name="levelSwitch">
        /// The level switch.
        /// </param>
        public DiagnosticsMiddleware(AppFunc next, LoggingLevelSwitch levelSwitch)
        {
            this.levelSwitch = levelSwitch;
            this.next = next;
        }

        /// <summary>
        /// The inject.
        /// </summary>
        /// <param name="env">
        /// The env.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public Task Inject(IDictionary<string, object> env)
        {
            var context = new OwinContext(env);
            if (context.Request.Path.StartsWithSegments(DiagnosticsPath))
            {
                return this.HandleDiagnosticsEndpoint(context);
            }

            return this.next(env);
        }

        /// <summary>
        /// The handle diagnostics endpoint.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private Task HandleDiagnosticsEndpoint(OwinContext context)
        {
            var logLevelFromQuery = context.Request.Query[LogLevelParameter];

            if (Enum.TryParse(logLevelFromQuery, true, out LogEventLevel logLevel))
            {
                this.levelSwitch.MinimumLevel = logLevel;
                context.Response.StatusCode = 204;
                return Task.FromResult(0);
            }

            context.Response.StatusCode = 400;
            return Task.FromResult(0);
        }
    }
}
