using System.Collections.Generic;

namespace Fabric.Databus.Config
{
    public class QueryConfig
    {
        public string Name { get; set; }

        public string Url { get; set; }
        public List<string> Urls => new List<string> {Url};

        public string ConnectionString { get; set; }

        public int EntitiesPerBatch { get; set; }
        public int MaximumEntitiesToLoad { get; set; }

        public string LocalSaveFolder { get; set; }

        public bool DropAndReloadIndex { get; set; }

        public bool WriteTemporaryFilesToDisk { get; set; }
        public bool WriteDetailedTemporaryFilesToDisk { get; set; }
        public bool KeepTemporaryLookupColumnsInOutput { get; set; }
        public int EntitiesPerUploadFile { get; set; }

        public string Index { get; set; }
        public string Alias { get; set; }
        public string EntityType { get; set; }

        public string TopLevelKeyColumn { get; set; }
        public bool UploadToElasticSearch { get; set; } = true;

        public bool CompressFiles { get; set; }
    }
}