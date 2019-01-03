namespace Fabric.Databus.API
{
    using Fabric.Databus.API.Configuration;
    using Fabric.Databus.API.Wrappers;
    using Fabric.Databus.Domain.ConfigValidators;
    using Fabric.Databus.Domain.Importers;
    using Fabric.Databus.Domain.Jobs;
    using Fabric.Databus.PipelineRunner;

    using Nancy;
    using Nancy.Bootstrapper;
    using Nancy.Configuration;
    using Nancy.TinyIoc;

    using Newtonsoft.Json;

    using Serilog;

    /// <inheritdoc />
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The _app config.
        /// </summary>
        private readonly IAppConfiguration appConfig;

        /// <inheritdoc />
        public Bootstrapper(ILogger logger, IAppConfiguration appConfig)
        {
            this.logger = logger;
            this.appConfig = appConfig;
        }

        /// <inheritdoc />
        public override void Configure(INancyEnvironment environment)
        {
            base.Configure(environment);

            environment.Tracing(enabled: false, displayErrorTraces: true);
        }

        /// <inheritdoc />
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            pipelines.OnError.AddItemToEndOfPipeline((ctx, ex) =>
            {
                this.logger.Error(ex, "Unhandled error on request: @{Url}. Error Message: @{Message}", ctx.Request.Url, ex.Message);
                return ctx.Response;
            });
            container.Register(this.logger);
            container.Register(this.appConfig);
            container.Register(this.appConfig.ElasticSearchSettings);

            // implement our own json serializer for pretty printing
            // Singleton Lifetimes
            container.Register<JsonSerializer, CustomJsonSerializer>();
            //// container.Register<IJobHistoryStore, ElasticSearchJobHistoryStore>();
            container.Register<IJobHistoryStore, InMemoryJobHistoryStore>();
            container.Register<IConfigValidator, MyConfigValidator>();
            container.Register<IJobStatusTrackerFactory, JobStatusTrackerFactory>();

            // Multi-instance lifetimes
            container.Register<IJobStatusTracker, JobStatusTracker>().AsMultiInstance();
            container.Register<IImportRunner, MyPipelineRunner>().AsMultiInstance();
            container.Register<IJobScheduler, JobScheduler>().AsMultiInstance();
        }
    }
}
