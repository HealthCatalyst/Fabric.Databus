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

namespace Fabric.Databus.API
{
		public class Startup
		{
				private readonly IConfiguration _config;
				private readonly IConfiguration _hostingConfiguration;

				public Startup(IHostingEnvironment env)
				{
						_config = new ConfigurationBuilder()
								.AddJsonFile("appsettings.json")
								.SetBasePath(env.ContentRootPath)
								.Build();

						_hostingConfiguration = new ConfigurationBuilder()
								.SetBasePath(Directory.GetCurrentDirectory())
								.AddJsonFile("hosting.json")
								.Build();
				}

				public void Configure(IApplicationBuilder app)
				{
						var appConfig = new AppConfiguration();
						_config.Bind(appConfig);
						var idServerSettings = appConfig.IdentityServerConfidentialClientSettings;

						var levelSwitch = new LoggingLevelSwitch();
						var log = ConfigureLogger(levelSwitch, appConfig);

						app.UseCors("default");
						app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
						{
								Authority = idServerSettings.Authority,
								RequireHttpsMetadata = false,

								ApiName = idServerSettings.ClientId,
						});

						app.UseOwin(buildFunc =>
						{
								buildFunc(next => GlobalErrorLoggingMiddleware.Inject(next, log));
								buildFunc(CorrelationTokenMiddleware.Inject);
								buildFunc(next => RequestLoggingMiddleware.Inject(next, log));
								buildFunc(next => PerformanceLoggingMiddleware.Inject(next, log));
								buildFunc(next => new DiagnosticsMiddleware(next, levelSwitch).Inject);
								buildFunc(next => new MonitoringMiddleware(next, HealthCheck).Inject);
								buildFunc.UseAuthPlatform(idServerSettings.Scopes);
								buildFunc.UseNancy(opt => opt.Bootstrapper = new Bootstrapper(log, appConfig));
						});
				}

				public void ConfigureServices(IServiceCollection services)
				{
						services.AddWebEncoders();
						services.AddCors(options =>
						{
								options.AddPolicy("default", policy =>
								{
										policy.WithOrigins(_hostingConfiguration.GetSection("urls").Value).AllowAnyHeader().AllowAnyMethod();
								});
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
