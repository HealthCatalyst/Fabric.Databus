// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DatabusHttpRequestLogger.cs" company="">
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
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces.FileWriters;
    using Fabric.Shared.ReliableHttp.Interfaces;

    /// <inheritdoc />
    /// <summary>
    /// The data bus http request logger.
    /// </summary>
    public class DatabusHttpRequestLogger : IHttpRequestLogger
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
        public DatabusHttpRequestLogger(ITemporaryFileWriter temporaryFileWriter, string localSaveFolder)
        {
            this.temporaryFileWriter = temporaryFileWriter ?? throw new ArgumentNullException(nameof(temporaryFileWriter));

            if (this.temporaryFileWriter?.IsWritingEnabled == true && localSaveFolder != null)
            {
                this.folder = this.temporaryFileWriter.CombinePath(localSaveFolder, $"HttpRequests");
            }
        }

        /// <inheritdoc />
        public async Task LogRequestAsync(string requestId, HttpMethod method, HttpRequestMessage request)
        {
            if (this.temporaryFileWriter?.IsWritingEnabled == true)
            {
                this.temporaryFileWriter.CreateDirectory(this.folder);

                using (var stream = new MemoryStream())
                {
                    using (var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                    {
                        await writer.WriteLineAsync(request.ToString());
                        await writer.WriteLineAsync();
                    }

                    await request.Content.CopyToAsync(stream);

                    var path = this.temporaryFileWriter.CombinePath(this.folder, requestId + ".txt");

                    await this.temporaryFileWriter.WriteStreamAsync(path, stream);
                }
            }
        }
    }
}
