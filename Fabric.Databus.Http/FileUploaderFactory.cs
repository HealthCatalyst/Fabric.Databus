// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElasticSearchUploaderFactory.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the ElasticSearchUploaderFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Http
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;

    using Fabric.Databus.Interfaces.Http;
    using Fabric.Shared.ReliableHttp.Interfaces;

    using Serilog;

    /// <inheritdoc />
    public class FileUploaderFactory : IFileUploaderFactory
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
        /// The http request interceptor.
        /// </summary>
        private readonly IHttpRequestInterceptor httpRequestInterceptor;

        /// <summary>
        /// The http response interceptor.
        /// </summary>
        private readonly IHttpResponseInterceptor httpResponseInterceptor;

        private readonly IHttpRequestLogger httpRequestLogger;
        private readonly IHttpResponseLogger httpResponseLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileUploaderFactory"/> class. 
        /// </summary>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="httpClientFactory">
        /// The http Client Factory.
        /// </param>
        /// <param name="httpRequestInterceptor">
        /// The http Request Interceptor.
        /// </param>
        /// <param name="httpResponseInterceptor">
        /// The http ResponseContent Interceptor.
        /// </param>
        /// <param name="httpRequestLogger"></param>
        /// <param name="httpResponseLogger"></param>
        public FileUploaderFactory(
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
        public IFileUploader Create(List<string> urls, HttpMethod method, CancellationToken cancellationToken)
        {
            return new FileUploader(this.logger, 
                urls, this.httpClientFactory, this.httpRequestInterceptor, this.httpResponseInterceptor, method,
                this.httpRequestLogger, this.httpResponseLogger,
                cancellationToken);
        }
    }
}