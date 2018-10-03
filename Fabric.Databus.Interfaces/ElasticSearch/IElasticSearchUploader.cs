// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IElasticSearchUploader.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the IElasticSearchUploader type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.ElasticSearch
{
    using System.IO;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces.Http;

    /// <summary>
    /// The FileUploader interface.
    /// </summary>
    public interface IElasticSearchUploader : IFileUploader
    {
        /// <summary>
        /// The create index and mappings.
        /// </summary>
        /// <param name="folder">
        /// The folder.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task CreateIndexAndMappings(string folder);

        /// <summary>
        /// The delete index.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task DeleteIndex();

        /// <summary>
        /// The start upload.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task StartUploadAsync();

        /// <summary>
        /// The finish upload.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task FinishUploadAsync();

        /// <summary>
        /// The setup alias.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task SetupAliasAsync();

        /// <summary>
        /// The upload all files in folder.
        /// </summary>
        /// <param name="folder">
        /// The folder.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task UploadAllFilesInFolder(string folder);

        /// <summary>
        /// The test elastic search connection.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task<string> TestElasticSearchConnection();

        /// <summary>
        /// The refresh index.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task RefreshIndex();

        /// <summary>
        /// The send data to hosts.
        /// </summary>
        /// <param name="batch">
        /// The batch.
        /// </param>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <param name="doLogContent">
        /// The do log content.
        /// </param>
        /// <param name="doCompress">
        /// The do compress.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task SendDataToHostsAsync(
            int batch,
            Stream stream,
            bool doLogContent,
            bool doCompress);

        /// <summary>
        /// The send main mapping file to hosts.
        /// </summary>
        /// <param name="batch">
        /// The batch.
        /// </param>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <param name="doLogContent">
        /// The do log content.
        /// </param>
        /// <param name="doCompress">
        /// The do compress.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task SendMainMappingFileToHostsAsync(
            int batch,
            Stream stream,
            bool doLogContent,
            bool doCompress);

        /// <summary>
        /// The send nested mapping file to hosts.
        /// </summary>
        /// <param name="batch">
        /// The batch.
        /// </param>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <param name="doLogContent">
        /// The do log content.
        /// </param>
        /// <param name="doCompress">
        /// The do compress.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task SendNestedMappingFileToHostsAsync(
            int batch,
            Stream stream,
            bool doLogContent,
            bool doCompress);
    }
}