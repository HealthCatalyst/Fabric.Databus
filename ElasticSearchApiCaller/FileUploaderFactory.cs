// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileUploaderFactory.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the FileUploaderFactory type.
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
    public class FileUploaderFactory : IFileUploaderFactory
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileUploaderFactory"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public FileUploaderFactory(ILogger logger)
        {
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public IFileUploader Create(string userName, string password, bool keepIndexOnline)
        {
            return new FileUploader(userName, password, keepIndexOnline, this.logger);
        }
    }
}
