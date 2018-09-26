namespace ElasticSearchSqlFeeder.Interfaces
{
    using System.Collections.Generic;

    public interface IQueryConfig
    {
        string Name { get; set; }

        string Url { get; set; }

        string ElasticSearchUserName { get; set; }

        string ElasticSearchPassword { get; set; }

        List<string> Urls { get; }

        string ConnectionString { get; set; }

        int EntitiesPerBatch { get; set; }

        int MaximumEntitiesToLoad { get; set; }

        string LocalSaveFolder { get; set; }

        bool DropAndReloadIndex { get; set; }

        bool WriteTemporaryFilesToDisk { get; set; }

        bool WriteDetailedTemporaryFilesToDisk { get; set; }

        bool KeepTemporaryLookupColumnsInOutput { get; set; }

        int EntitiesPerUploadFile { get; set; }

        string Index { get; set; }

        string Alias { get; set; }

        string EntityType { get; set; }

        string TopLevelKeyColumn { get; set; }

        bool UploadToElasticSearch { get; set; }

        bool CompressFiles { get; set; }

        int SqlCommandTimeoutInSeconds { get; set; }

        bool KeepIndexOnline { get; set; }
    }
}