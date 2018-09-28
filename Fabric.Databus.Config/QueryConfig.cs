// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryConfig.cs" company="">
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

    using ElasticSearchSqlFeeder.Interfaces;

    /// <summary>
    /// The query config.
    /// </summary>
    [DataContract(Name = "Config", Namespace = "")]
    public class QueryConfig : IQueryConfig
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public string ElasticSearchUserName { get; set; }

        [DataMember]
        public string ElasticSearchPassword { get; set; }

        [DataMember]
        public List<string> Urls => new List<string> {Url};

        [DataMember]
        public string ConnectionString { get; set; }

        [DataMember]
        public int EntitiesPerBatch { get; set; }

        [DataMember]
        public int MaximumEntitiesToLoad { get; set; }

        [DataMember]
        public string LocalSaveFolder { get; set; }

        [DataMember]
        public bool DropAndReloadIndex { get; set; }

        [DataMember]
        public bool WriteTemporaryFilesToDisk { get; set; }

        [DataMember]
        public bool WriteDetailedTemporaryFilesToDisk { get; set; }

        [DataMember]
        public bool KeepTemporaryLookupColumnsInOutput { get; set; }

        [DataMember]
        public int EntitiesPerUploadFile { get; set; }

        [DataMember]
        public string Index { get; set; }

        [DataMember]
        public string Alias { get; set; }

        [DataMember]
        public string EntityType { get; set; }

        [DataMember]
        public string TopLevelKeyColumn { get; set; }

        [DataMember]
        public bool UploadToElasticSearch { get; set; } = true;

        [DataMember]
        public bool CompressFiles { get; set; }

        [DataMember]
        public int SqlCommandTimeoutInSeconds { get; set; }

        [DataMember]
        public bool KeepIndexOnline { get; set; }
    }
}