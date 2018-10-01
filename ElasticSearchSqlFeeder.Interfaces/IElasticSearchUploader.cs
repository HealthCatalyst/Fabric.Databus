// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IElasticSearchUploader.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IElasticSearchUploader type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ElasticSearchSqlFeeder.Interfaces
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// The FileUploader interface.
    /// </summary>
    public interface IElasticSearchUploader
    {
        Task CreateIndexAndMappings(List<string> hosts, string index, string alias, string entity, string folder);

        Task DeleteIndex(List<string> hosts, string index, string alias);

        Task StartUpload(List<string> hosts, string index, string alias);

        Task FinishUpload(List<string> hosts, string index, string alias);

        Task SetupAlias(List<string> hosts, string indexName, string aliasName);

        Task UploadAllFilesInFolder(List<string> hosts, string index, string alias, string entity, string folder);

        Task SendStreamToHosts(List<string> hosts, string relativeUrl, int batch, Stream stream, bool doLogContent, bool doCompress);

        Task<string> TestElasticSearchConnection(List<string> hosts);

        Task RefreshIndex(List<string> hosts, string index, string alias);

        /// <summary>
        /// The send data to hosts.
        /// </summary>
        /// <param name="hosts">
        /// The hosts.
        /// </param>
        /// <param name="indexName">
        /// The index name.
        /// </param>
        /// <param name="entityType">
        /// The entity type.
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
        Task SendDataToHosts(
            List<string> hosts,
            string indexName,
            string entityType,
            int batch,
            Stream stream,
            bool doLogContent,
            bool doCompress);

        /// <summary>
        /// The send main mapping file to hosts.
        /// </summary>
        /// <param name="hosts">
        /// The hosts.
        /// </param>
        /// <param name="indexName">
        /// The index name.
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
        Task SendMainMappingFileToHosts(
            List<string> hosts,
            string indexName,
            int batch,
            Stream stream,
            bool doLogContent,
            bool doCompress);

        /// <summary>
        /// The send nested mapping file to hosts.
        /// </summary>
        /// <param name="hosts">
        /// The hosts.
        /// </param>
        /// <param name="indexName">
        /// The index name.
        /// </param>
        /// <param name="entityType">
        /// The entity type.
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
        Task SendNestedMappingFileToHosts(
            List<string> hosts,
            string indexName,
            string entityType,
            int batch,
            Stream stream,
            bool doLogContent,
            bool doCompress);
    }
}