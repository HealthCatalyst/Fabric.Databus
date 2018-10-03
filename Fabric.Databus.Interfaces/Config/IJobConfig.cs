// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IJobConfig.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IJobConfig type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Config
{
    /// <summary>
    /// The JobConfig interface.
    /// </summary>
    public interface IJobConfig
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the entities per batch.
        /// </summary>
        int EntitiesPerBatch { get; set; }

        /// <summary>
        /// Gets or sets the maximum entities to load.
        /// </summary>
        int MaximumEntitiesToLoad { get; set; }

        /// <summary>
        /// Gets or sets the local save folder.
        /// </summary>
        string LocalSaveFolder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether write temporary files to disk.
        /// </summary>
        bool WriteTemporaryFilesToDisk { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether write detailed temporary files to disk.
        /// </summary>
        bool WriteDetailedTemporaryFilesToDisk { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether keep temporary lookup columns in output.
        /// </summary>
        bool KeepTemporaryLookupColumnsInOutput { get; set; }

        /// <summary>
        /// Gets or sets the entities per upload file.
        /// </summary>
        int EntitiesPerUploadFile { get; set; }

        /// <summary>
        /// Gets or sets the top level key column.
        /// </summary>
        string TopLevelKeyColumn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether compress files.
        /// </summary>
        bool CompressFiles { get; set; }

        /// <summary>
        /// Gets or sets the sql command timeout in seconds.
        /// </summary>
        int SqlCommandTimeoutInSeconds { get; set; }

        /// <summary>
        /// Gets or sets the entity type.
        /// </summary>
        string EntityType { get; set; }
    }
}