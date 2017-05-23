﻿using System;
using ElasticSearchSqlFeeder.Shared;
using Fabric.Databus.Config;
using Fabric.Databus.Domain.Jobs;
using Fabric.Shared;
using Nancy;
using Nancy.Extensions;
using Nancy.IO;
using Serilog;
using SqlImporter;

namespace Fabric.Databus.API.Modules
{
    public class JobModule : NancyModule
    {
        public JobModule(ILogger logger, IJobScheduler jobScheduler) : base("/jobs")
        {
            Post("/", parameters =>
            {
                var body = RequestStream.FromStream(Request.Body).AsString();
                var queryConfig = body.FromXml<Job>();

                var jobId= jobScheduler.ExecuteJobImmediately(queryConfig);

                var uriBuilder = new UriBuilder(Request.Url.Scheme, Request.Url.HostName,
                    Request.Url.Port ?? 80, $"jobstatus/{jobId}");

                var statusUri = uriBuilder.ToString();

                var model = new
                {
                    JobId = jobId,
                    links = new []
                    {
                        new
                        {
                            status = statusUri
                        }
                    }
                };

                return Negotiate
                    .WithModel( model)
                    .WithStatusCode(HttpStatusCode.Accepted)
                    .WithHeader("Location", statusUri);
            });

        }
        
    }
}