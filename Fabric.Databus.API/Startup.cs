using System;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Fabric.Databus.API.Configuration;
using Fabric.Databus.API.Middleware;
using Fabric.Databus.Domain.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Nancy;
using Nancy.Owin;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.Elasticsearch;

namespace Fabric.Databus.API
{
    public class Startup
    {
        private readonly IConfiguration _config;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .SetBasePath(env.ContentRootPath);

            _config = builder.Build();
        }
        public void Configure(IApplicationBuilder app)
        {
            var appConfig = new AppConfiguration();
            ConfigurationBinder.Bind(_config, appConfig);

            var levelSwitch = new LoggingLevelSwitch();
            var log = ConfigureLogger(levelSwitch, appConfig);


            app.UseOwin(buildFunc =>
            {
                buildFunc(next => GlobalErrorLoggingMiddleware.Inject(next, log));
                buildFunc(CorrelationTokenMiddleware.Inject);
                buildFunc(next => RequestLoggingMiddleware.Inject(next, log));
                buildFunc(next => PerformanceLoggingMiddleware.Inject(next, log));
                buildFunc(next => new DiagnosticsMiddleware(next, levelSwitch).Inject);
                buildFunc(next => new MonitoringMiddleware(next, HealthCheck).Inject);
                buildFunc.UseNancy(opt => opt.Bootstrapper = new Bootstrapper(log, appConfig));
            });
        }

        public Task<bool> HealthCheck()
        {
            //TODO: dummy deep health check for now
            return Task.FromResult(true);
        }

        private ILogger ConfigureLogger(LoggingLevelSwitch levelSwitch, IAppConfiguration appConfig)
        {

            var sinkOptions = new ElasticsearchSinkOptions(CreateElasticSearchUri(appConfig.ElasticSearchSettings))
            {
                IndexFormat = "logstash-fabricdatabus-{0:yyyy.MM.dd}"
            };
            return new LoggerConfiguration()
                .MinimumLevel.ControlledBy(levelSwitch)
                .Enrich.FromLogContext()
                .WriteTo
                .Elasticsearch(sinkOptions).CreateLogger();
        }

        private Uri CreateElasticSearchUri(ElasticSearchSettings elasticSearchConfig)
        {
            if (string.IsNullOrEmpty(elasticSearchConfig.Scheme) || string.IsNullOrEmpty(elasticSearchConfig.Server) ||
                string.IsNullOrEmpty(elasticSearchConfig.Port))
            {
                throw new ConfigurationException("You must specify Scheme, Server and Port for elastic search.");
            }

            if (!string.IsNullOrEmpty(elasticSearchConfig.Username) &&
                !string.IsNullOrEmpty(elasticSearchConfig.Password))
            {
                return new Uri(
                    $"{elasticSearchConfig.Scheme}://{elasticSearchConfig.Username}:{elasticSearchConfig.Password}@{elasticSearchConfig.Server}:{elasticSearchConfig.Port}");
            }

            return new Uri($"{elasticSearchConfig.Scheme}://{elasticSearchConfig.Server}:{elasticSearchConfig.Port}");


        }
    }
}
