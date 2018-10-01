namespace ElasticSearchSqlFeeder.Interfaces
{
    public interface IJobConfig
    {
        string Name { get; set; }

        int EntitiesPerBatch { get; set; }

        int MaximumEntitiesToLoad { get; set; }

        string LocalSaveFolder { get; set; }

        bool WriteTemporaryFilesToDisk { get; set; }

        bool WriteDetailedTemporaryFilesToDisk { get; set; }

        bool KeepTemporaryLookupColumnsInOutput { get; set; }

        int EntitiesPerUploadFile { get; set; }

        string TopLevelKeyColumn { get; set; }

        bool CompressFiles { get; set; }

        int SqlCommandTimeoutInSeconds { get; set; }
    }
}