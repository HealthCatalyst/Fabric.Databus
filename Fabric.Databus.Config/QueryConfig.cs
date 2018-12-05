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
    using System.Net.Http;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    using Fabric.Databus.Interfaces.Config;

    /// <inheritdoc />
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
        public string UrlUserName { get; set; }

        /// <inheritdoc />
        [DataMember]
        public string UrlPassword { get; set; }

        /// <inheritdoc />
        [DataMember]
        [XmlIgnore]
        public HttpMethod UrlMethod { get; set; } = HttpMethod.Put;

        /// <summary>
        /// Gets or sets the my url method.
        /// </summary>
        [XmlElement("UrlMethod")]
        public string UrlMethodProxy
        {
            get => this.UrlMethod.ToString();
            set
            {
                switch (value)
                {
                    case nameof(HttpMethod.Put):
                        this.UrlMethod = HttpMethod.Put;
                        break;

                    default:
                        this.UrlMethod = HttpMethod.Post;
                        break;
                }
            }
        }

        /// <inheritdoc />
        [DataMember]
        public List<string> Urls => new List<string> { this.Url };

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
        public bool UploadToUrl { get; set; } = true;

        /// <inheritdoc />
        [DataMember]
        public bool CompressFiles { get; set; }

        /// <inheritdoc />
        [DataMember]
        public int SqlCommandTimeoutInSeconds { get; set; } = DefaultSqlCommandTimeoutInSeconds;

        /// <inheritdoc />
        [DataMember]
        public bool KeepIndexOnline { get; set; }

        /// <inheritdoc />
        [DataMember]
        public bool UseMultipleThreads { get; set; }

        /// <inheritdoc />
        public PipelineNames Pipeline { get; set; } = PipelineNames.RestApi;
    }
}