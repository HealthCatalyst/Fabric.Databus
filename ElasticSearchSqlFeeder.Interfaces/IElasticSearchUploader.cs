// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IElasticSearchUploader.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the IElasticSearchUploader type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces
{
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// The FileUploader interface.
    /// </summary>
    public interface IElasticSearchUploader
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
        Task StartUpload();

        /// <summary>
        /// The finish upload.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task FinishUpload();

        /// <summary>
        /// The setup alias.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task SetupAlias();

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
        /// The send stream to hosts.
        /// </summary>
        /// <param name="relativeUrl">
        /// The relative url.
        /// </param>
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
        Task SendStreamToHosts(string relativeUrl, int batch, Stream stream, bool doLogContent, bool doCompress);

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
        Task SendDataToHosts(
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
        Task SendMainMappingFileToHosts(
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
        Task SendNestedMappingFileToHosts(
            int batch,
            Stream stream,
            bool doLogContent,
            bool doCompress);
    }
}