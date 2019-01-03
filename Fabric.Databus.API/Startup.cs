// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Startup.cs" company="">
//   
// </copyright>
// <summary>
//   The startup.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.API
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Fabric.Databus.API.Configuration;
    using Fabric.Databus.API.Middleware;
    using Fabric.Databus.Domain.Configuration;
    using Fabric.Platform.Auth;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Nancy;
    using Nancy.Owin;

    using Serilog;
    using Serilog.Core;
    using Serilog.Sinks.Elasticsearch;

    /// <summary>
    /// The startup.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// The config.
        /// </summary>
        private readonly IConfiguration config;

        /// <summary>
        /// The _hosting configuration.
        /// </summary>
        private readonly IConfiguration hostingConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">
        /// The env.
        /// </param>
        public Startup(IHostingEnvironment env)
        {
            this.config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .SetBasePath(env.ContentRootPath)
                    .AddEnvironmentVariables()
                    .Build();

            this.hostingConfiguration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("hosting.json")
                    .AddEnvironmentVariables()
                    .Build();
        }

        /// <summary>
        /// The configure.
        /// </summary>
        /// <param name="app">
        /// The app.
        /// </param>
        public void Configure(IApplicationBuilder app)
        {
            var appConfig = new AppConfiguration();
            this.config.Bind(appConfig);

            Console.WriteLine("Configuring with EnableAuthorization=" + appConfig.EnableAuthorization);

            var levelSwitch = new LoggingLevelSwitch();
            var log = this.ConfigureLogger(levelSwitch, appConfig);

            app.UseCors("default");
            app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
            {
                Authority = appConfig.Authority,
                RequireHttpsMetadata = false,

                ApiName = appConfig.ClientId,
            });

            app.UseOwin(buildFunc =>
            {
                buildFunc(next => GlobalErrorLoggingMiddleware.Inject(next, log));
                buildFunc(CorrelationTokenMiddleware.Inject);
                buildFunc(next => RequestLoggingMiddleware.Inject(next, log));
                buildFunc(next => PerformanceLoggingMiddleware.Inject(next, log));
                buildFunc(next => new DiagnosticsMiddleware(next, levelSwitch).Inject);
                buildFunc(next => new MonitoringMiddleware(next, HealthCheck).Inject);
                if (appConfig.EnableAuthorization)
                {
                    buildFunc.UseAuthPlatform(appConfig.Scopes.Split(','));
                }
                buildFunc.UseNancy(opt => opt.Bootstrapper = new Bootstrapper(log, appConfig));
            });
        }

        /// <summary>
        /// The configure services.
        /// </summary>
        /// <param name="services">
        /// The services.
        /// </param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWebEncoders();
            services.AddCors(options =>
                {
                    options.AddPolicy(
                        "default",
                        policy =>
                            {
                                policy.WithOrigins(this.hostingConfiguration.GetSection("urls").Value).AllowAnyHeader().AllowAnyMethod();
                    });
            });
        }

        /// <summary>
        /// The health check.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public Task<bool> HealthCheck()
        {
            // TODO: dummy deep health check for now
            return Task.FromResult(true);
        }

        /// <summary>
        /// The configure logger.
        /// </summary>
        /// <param name="levelSwitch">
        /// The level switch.
        /// </param>
        /// <param name="appConfig">
        /// The app config.
        /// </param>
        /// <returns>
        /// The <see cref="ILogger"/>.
        /// </returns>
        private ILogger ConfigureLogger(LoggingLevelSwitch levelSwitch, IAppConfiguration appConfig)
        {

            var sinkOptions = new ElasticsearchSinkOptions(this.CreateElasticSearchUri(appConfig.ElasticSearchSettings))
            {
                // ReSharper disable once StringLiteralTypo
                IndexFormat = @"logstash-fabricdatabus-{0:yyyy.MM.dd}"
            };
            return new LoggerConfiguration()
                    .MinimumLevel.ControlledBy(levelSwitch)
                    .Enrich.FromLogContext()
                    .WriteTo
                    .Elasticsearch(sinkOptions).CreateLogger();
        }

        /// <summary>
        /// The create elastic search uri.
        /// </summary>
        /// <param name="elasticSearchConfig">
        /// The elastic search config.
        /// </param>
        /// <returns>
        /// The <see cref="Uri"/>.
        /// </returns>
        /// <exception cref="ConfigurationException">
        /// configuration exception
        /// </exception>
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
