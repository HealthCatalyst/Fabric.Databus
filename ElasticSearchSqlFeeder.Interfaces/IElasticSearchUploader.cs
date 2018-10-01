// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IElasticSearchUploader.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the IElasticSearchUploader type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ElasticSearchSqlFeeder.Interfaces
{
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// The FileUploader interface.
    /// </summary>
    public interface IElasticSearchUploader
    {
        Task CreateIndexAndMappings(string folder);

        Task DeleteIndex();

        Task StartUpload();

        Task FinishUpload();

        Task SetupAlias();

        Task UploadAllFilesInFolder(string folder);

        Task SendStreamToHosts(string relativeUrl, int batch, Stream stream, bool doLogContent, bool doCompress);

        Task<string> TestElasticSearchConnection();

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