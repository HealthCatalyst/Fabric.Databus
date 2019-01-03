// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MonitoringMiddleware.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the MonitoringMiddleware type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.API.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LibOwin;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    /// <summary>
    /// The monitoring middleware.
    /// </summary>
    public class MonitoringMiddleware
    {
        /// <summary>
        /// The monitor path.
        /// </summary>
        private static readonly PathString MonitorPath = new PathString("/_monitor");

        /// <summary>
        /// The monitor shallow path.
        /// </summary>
        private static readonly PathString MonitorShallowPath = new PathString("/_monitor/shallow");

        /// <summary>
        /// The monitor deep path.
        /// </summary>
        private static readonly PathString MonitorDeepPath = new PathString("/_monitor/deep");

        /// <summary>
        /// The next.
        /// </summary>
        private readonly AppFunc next;

        /// <summary>
        /// The _health check.
        /// </summary>
        private readonly Func<Task<bool>> healthCheck;

        /// <summary>
        /// Initializes a new instance of the <see cref="MonitoringMiddleware"/> class.
        /// </summary>
        /// <param name="next">
        /// The next.
        /// </param>
        /// <param name="healthCheck">
        /// The health check.
        /// </param>
        public MonitoringMiddleware(AppFunc next, Func<Task<bool>> healthCheck)
        {
            this.next = next;
            this.healthCheck = healthCheck;
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
            if (context.Request.Path.StartsWithSegments(MonitorPath))
            {
                return this.HandleMonitorEndpoint(context);
            }

            return this.next(env);
        }

        /// <summary>
        /// The handle monitor endpoint.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private Task HandleMonitorEndpoint(OwinContext context)
        {
            if (context.Request.Path.StartsWithSegments(MonitorShallowPath))
            {
                context.Response.StatusCode = 204;
                return Task.FromResult(0);
            }

            if (context.Request.Path.StartsWithSegments(MonitorDeepPath))
            {
                return this.HandleDeepEndpoint(context);
            }

            context.Response.StatusCode = 404;
            return Task.FromResult(0);
        }

        /// <summary>
        /// The handle deep endpoint.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task HandleDeepEndpoint(OwinContext context)
        {
            if (await this.healthCheck())
            {
                context.Response.StatusCode = 204;
            }
            else
            {
                context.Response.StatusCode = 503;
            }
        }
    }
}
