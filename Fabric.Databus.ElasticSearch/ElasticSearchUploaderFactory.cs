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

    using Fabric.Databus.Interfaces;

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
        /// Initializes a new instance of the <see cref="ElasticSearchUploaderFactory"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="httpClientFactory">
        /// The http Client Factory.
        /// </param>
        public ElasticSearchUploaderFactory(ILogger logger, IHttpClientFactory httpClientFactory)
        {
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        /// <inheritdoc />
        public IElasticSearchUploader Create(string userName, string password, bool keepIndexOnline, List<string> urls, string index, string alias, string entityType)
        {
            return new ElasticSearchUploader(userName, password, keepIndexOnline, this.logger, urls, index, alias, entityType, this.httpClientFactory);
        }
    }
}