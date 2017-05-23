using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LibOwin;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Fabric.Databus.API.Middleware
{
    public class MonitoringMiddleware
    {
        private readonly AppFunc _next;
        private readonly Func<Task<bool>> _healthCheck;

        private static readonly PathString MonitorPath = new PathString("/_monitor");
        private static readonly PathString MonitorShallowPath= new PathString("/_monitor/shallow");
        private static readonly PathString MonitorDeepPath = new PathString("/_monitor/deep");

        public MonitoringMiddleware(AppFunc next, Func<Task<bool>> healthCheck)
        {
            _next = next;
            _healthCheck = healthCheck;
        }

        public Task Inject(IDictionary<string, object> env)
        {
            var context = new OwinContext(env);
            if (context.Request.Path.StartsWithSegments(MonitorPath))
            {
                return HandleMonitorEndpoint(context);
            }
            return _next(env);
        }

        private Task HandleMonitorEndpoint(OwinContext context)
        {
            if (context.Request.Path.StartsWithSegments(MonitorShallowPath))
            {
                context.Response.StatusCode = 204;
                return Task.FromResult(0);
            }
            if (context.Request.Path.StartsWithSegments(MonitorDeepPath))
            {
                return HandleDeepEndpoint(context);
            }
            context.Response.StatusCode = 404;
            return Task.FromResult(0);
        }

        private async Task HandleDeepEndpoint(OwinContext context)
        {
            if (await _healthCheck())
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
