// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFileUploader.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IFileUploader type.
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
    public interface IFileUploader
    {
        Task CreateIndexAndMappings(List<string> hosts, string index, string alias, string entity, string folder);

        Task DeleteIndex(List<string> hosts, string relativeUrl, string index, string alias);

        Task StartUpload(List<string> hosts, string index, string alias);

        Task FinishUpload(List<string> hosts, string index, string alias);

        Task SetupAlias(List<string> hosts, string indexName, string aliasName);

        Task UploadAllFilesInFolder(List<string> hosts, string index, string alias, string entity, string folder);

        Task SendStreamToHosts(List<string> hosts, string relativeUrl, int batch, Stream stream, bool doLogContent, bool doCompress);

        Task<string> TestElasticSearchConnection(List<string> hosts);

        Task RefreshIndex(List<string> hosts, string index, string alias);
    }
}