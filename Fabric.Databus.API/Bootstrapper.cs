using Fabric.Databus.API.Configuration;
using Fabric.Databus.Domain.ConfigValidators;
using Fabric.Databus.Domain.Importers;
using Fabric.Databus.Domain.Jobs;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Configuration;
using Nancy.TinyIoc;
using Newtonsoft.Json;
using Serilog;
using SqlImporter;

namespace Fabric.Databus.API
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        private readonly ILogger _logger;
        private readonly IAppConfiguration _appConfig;

        public Bootstrapper(ILogger logger, IAppConfiguration appConfig)
        {
            _logger = logger;
            _appConfig = appConfig;
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            pipelines.OnError.AddItemToEndOfPipeline((ctx, ex) =>
            {
                _logger.Error(ex, "Unhandled error on request: @{Url}. Error Message: @{Message}", ctx.Request.Url, ex.Message);
                return ctx.Response;
            });
            container.Register(_logger);
            container.Register(_appConfig.ElasticSearchSettings);

            // implement our own json serializer for pretty printing
            //Singleton Lifetimes
            container.Register<JsonSerializer, CustomJsonSerializer>();
            //container.Register<IJobHistoryStore, ElasticSearchJobHistoryStore>();
            container.Register<IJobHistoryStore, InMemoryJobHistoryStore>();
            container.Register<IConfigValidator, ConfigValidator>();
            container.Register<IJobStatusTrackerFactory, JobStatusTrackerFactory>();

            //Multi-instance lifetimes
            container.Register<IJobStatusTracker, JobStatusTracker>().AsMultiInstance();
            container.Register<IImportRunner, SqlImportRunnerSimple>().AsMultiInstance();
            container.Register<IJobScheduler, JobScheduler>().AsMultiInstance();

        }

        public override void Configure(INancyEnvironment environment)
        {
            base.Configure(environment);

            environment.Tracing(enabled: false, displayErrorTraces: true);
        }
    }
}
