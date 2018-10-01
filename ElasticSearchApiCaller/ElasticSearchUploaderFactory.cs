// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElasticSearchUploaderFactory.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the ElasticSearchUploaderFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ElasticSearchApiCaller
{
    using System.Collections.Generic;

    using ElasticSearchSqlFeeder.Interfaces;

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
        /// Initializes a new instance of the <see cref="ElasticSearchUploaderFactory"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public ElasticSearchUploaderFactory(ILogger logger)
        {
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public IElasticSearchUploader Create(string userName, string password, bool keepIndexOnline, List<string> urls, string index, string alias, string entityType)
        {
            return new ElasticSearchUploader(userName, password, keepIndexOnline, this.logger, urls, index, alias, entityType);
        }
    }
}
