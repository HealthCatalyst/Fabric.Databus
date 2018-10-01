// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElasticSearchUploaderFactory.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ElasticSearchUploaderFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ElasticSearchApiCaller
{
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
        public IElasticSearchUploader Create(string userName, string password, bool keepIndexOnline)
        {
            return new ElasticSearchUploader(userName, password, keepIndexOnline, this.logger);
        }
    }
}
