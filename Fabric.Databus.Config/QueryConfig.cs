// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryConfig.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the QueryConfig type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Config
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    using Fabric.Databus.Interfaces.Config;

    /// <summary>
    /// The query config.
    /// </summary>
    [DataContract(Name = "Config", Namespace = "")]
    public class QueryConfig : IQueryConfig
    {
        /// <summary>
        /// The default sql command timeout in seconds.
        /// </summary>
        private const int DefaultSqlCommandTimeoutInSeconds = 120;


        /// <inheritdoc />
        [DataMember]
        public string Name { get; set; }

        /// <inheritdoc />
        [DataMember]
        public string Url { get; set; }

        /// <inheritdoc />
        [DataMember]
        public string ElasticSearchUserName { get; set; }

        /// <inheritdoc />
        [DataMember]
        public string ElasticSearchPassword { get; set; }

        /// <inheritdoc />
        [DataMember]
        public List<string> Urls => new List<string> { this.Url};

        /// <inheritdoc />
        [DataMember]
        public string ConnectionString { get; set; }

        /// <inheritdoc />
        [DataMember]
        public int EntitiesPerBatch { get; set; }

        /// <inheritdoc />
        [DataMember]
        public int MaximumEntitiesToLoad { get; set; }

        /// <inheritdoc />
        [DataMember]
        public string LocalSaveFolder { get; set; }

        /// <inheritdoc />
        [DataMember]
        public bool DropAndReloadIndex { get; set; }

        /// <inheritdoc />
        [DataMember]
        public bool WriteTemporaryFilesToDisk { get; set; }

        /// <inheritdoc />
        [DataMember]
        public bool WriteDetailedTemporaryFilesToDisk { get; set; }

        /// <inheritdoc />
        [DataMember]
        public bool KeepTemporaryLookupColumnsInOutput { get; set; }

        /// <inheritdoc />
        [DataMember]
        public int EntitiesPerUploadFile { get; set; }

        /// <inheritdoc />
        [DataMember]
        public string Index { get; set; }

        /// <inheritdoc />
        [DataMember]
        public string Alias { get; set; }

        /// <inheritdoc />
        [DataMember]
        public string EntityType { get; set; }

        /// <inheritdoc />
        [DataMember]
        public string TopLevelKeyColumn { get; set; }

        /// <inheritdoc />
        [DataMember]
        public bool UploadToElasticSearch { get; set; }

        /// <inheritdoc />
        [DataMember]
        public bool CompressFiles { get; set; }

        /// <inheritdoc />
        [DataMember]
        public int SqlCommandTimeoutInSeconds { get; set; } = DefaultSqlCommandTimeoutInSeconds;

        /// <inheritdoc />
        [DataMember]
        public bool KeepIndexOnline { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether use multiple threads.
        /// </summary>
        [DataMember]
        public bool UseMultipleThreads { get; set; }
    }
}