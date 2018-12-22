// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DatabusHttpResponseLogger.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the DatabusHttpRequestLogger type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared.Loggers
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces.FileWriters;
    using Fabric.Shared.ReliableHttp.Interfaces;

    /// <inheritdoc />
    /// <summary>
    /// The data bus http request logger.
    /// </summary>
    public class DatabusHttpResponseLogger : IHttpResponseLogger
    {
        /// <summary>
        /// The temporary file writer.
        /// </summary>
        private readonly ITemporaryFileWriter temporaryFileWriter;

        /// <summary>
        /// The folder.
        /// </summary>
        private readonly string folder;

        /// <inheritdoc />
        public DatabusHttpResponseLogger(ITemporaryFileWriter temporaryFileWriter, string localSaveFolder)
        {
            this.temporaryFileWriter = temporaryFileWriter;

            if (this.temporaryFileWriter?.IsWritingEnabled == true && localSaveFolder != null)
            {
                this.folder = this.temporaryFileWriter.CombinePath(localSaveFolder, $"HttpResponses");
            }
        }

        /// <inheritdoc />
        public async Task LogResponseAsync(
            string requestId,
            HttpMethod httpMethod,
            Uri fullUri,
            Stream requestContent,
            HttpStatusCode responseStatusCode,
            HttpContent responseContent,
            long stopwatchElapsedMilliseconds)
        {
            if (this.temporaryFileWriter?.IsWritingEnabled == true)
            {
                this.temporaryFileWriter.CreateDirectory(this.folder);

                using (var stream = new MemoryStream())
                {
                    using (var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                    {
                        await writer.WriteLineAsync($"{httpMethod.ToString()}");
                        await writer.WriteLineAsync($"{fullUri}");
                        await writer.WriteLineAsync($"{responseStatusCode}");
                        await writer.WriteLineAsync($"{stopwatchElapsedMilliseconds}");
                        await writer.WriteLineAsync();
                    }

                    await responseContent.CopyToAsync(stream);

                    var path = this.temporaryFileWriter.CombinePath(this.folder, requestId + ".txt");

                    await this.temporaryFileWriter.WriteStreamAsync(path, stream);
                }
            }
        }
    }
}
