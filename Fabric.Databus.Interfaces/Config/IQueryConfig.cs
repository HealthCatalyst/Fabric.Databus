// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IQueryConfig.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the IQueryConfig type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Config
{
    using System.Collections.Generic;

    /// <inheritdoc />
    /// <summary>
    /// The QueryConfig interface.
    /// </summary>
    public interface IQueryConfig : IJobConfig
    {
        /// <summary>
        /// Gets or sets the url.
        /// </summary>
        string Url { get; set; }

        /// <summary>
        /// Gets or sets the elastic search user name.
        /// </summary>
        string ElasticSearchUserName { get; set; }

        /// <summary>
        /// Gets or sets the elastic search password.
        /// </summary>
        string ElasticSearchPassword { get; set; }

        /// <summary>
        /// Gets the urls.
        /// </summary>
        List<string> Urls { get; }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether drop and reload index.
        /// </summary>
        bool DropAndReloadIndex { get; set; }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        string Index { get; set; }

        /// <summary>
        /// Gets or sets the alias.
        /// </summary>
        string Alias { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether upload to elastic search.
        /// </summary>
        bool UploadToElasticSearch { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether keep index online.
        /// </summary>
        bool KeepIndexOnline { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether use multiple threads.
        /// </summary>
        bool UseMultipleThreads { get; set; }

        /// <summary>
        /// Gets or sets the pipeline.
        /// </summary>
        PipelineNames Pipeline { get; set; }
    }
}