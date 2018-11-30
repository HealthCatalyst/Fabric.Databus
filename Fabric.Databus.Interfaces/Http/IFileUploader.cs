// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFileUploader.cs" company="">
//   
// </copyright>
// <summary>
//   The FileUploader interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Http
{
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;

    /// <summary>
    /// The FileUploader interface.
    /// </summary>
    public interface IFileUploader
    {
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
        Task<HttpStatusCode> SendStreamToHosts(string relativeUrl, int batch, Stream stream, bool doLogContent, bool doCompress);
    }
}