// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElasticSearchUploaderFactory.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the ElasticSearchUploaderFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.ElasticSearch
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading;

    using Fabric.Databus.Interfaces.ElasticSearch;
    using Fabric.Databus.Interfaces.Http;
    using Fabric.Shared.ReliableHttp.Interfaces;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The file uploader factory.
    /// </summary>
    public class ElasticSearchUploaderFactory : IElasticSearchUploaderFactory
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The http client factory.
        /// </summary>
        private readonly IHttpClientFactory httpClientFactory;

        /// <summary>
        /// The http request injector.
        /// </summary>
        private readonly IHttpRequestInterceptor httpRequestInterceptor;

        /// <summary>
        /// The http response interceptor.
        /// </summary>
        private readonly IHttpResponseInterceptor httpResponseInterceptor;

        private readonly IHttpRequestLogger httpRequestLogger;
        private readonly IHttpResponseLogger httpResponseLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElasticSearchUploaderFactory"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="httpClientFactory">
        /// The http Client Factory.
        /// </param>
        /// <param name="httpRequestInterceptor">
        /// The http Request Injector.
        /// </param>
        /// <param name="httpResponseInterceptor">
        /// The http response interceptor</param>
        /// <param name="httpRequestLogger"></param>
        /// <param name="httpResponseLogger"></param>
        public ElasticSearchUploaderFactory(
            ILogger logger,
            IHttpClientFactory httpClientFactory,
            IHttpRequestInterceptor httpRequestInterceptor,
            IHttpResponseInterceptor httpResponseInterceptor,
            IHttpRequestLogger httpRequestLogger,
            IHttpResponseLogger httpResponseLogger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            this.httpRequestInterceptor = httpRequestInterceptor;
            this.httpResponseInterceptor = httpResponseInterceptor;
            this.httpRequestLogger = httpRequestLogger ?? throw new ArgumentNullException(nameof(httpRequestLogger));
            this.httpResponseLogger = httpResponseLogger ?? throw new ArgumentNullException(nameof(httpResponseLogger));
        }

        /// <inheritdoc />
        public IElasticSearchUploader Create(
            bool keepIndexOnline,
            List<string> urls,
            string index,
            string alias,
            string entityType,
            HttpMethod method)
        {
            return new ElasticSearchUploader(
                keepIndexOnline,
                this.logger,
                urls,
                index,
                alias,
                entityType,
                this.httpClientFactory,
                this.httpRequestInterceptor,
                this.httpResponseInterceptor,
                method,
                this.httpRequestLogger,
                this.httpResponseLogger,
                CancellationToken.None);
        }
    }
}