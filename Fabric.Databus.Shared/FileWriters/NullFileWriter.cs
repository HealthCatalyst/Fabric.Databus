// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NullFileWriter.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the NullFileWriter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared.FileWriters
{
    using System.IO;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.FileWriters;

    /// <summary>
    /// The null file writer.
    /// </summary>
    public class NullFileWriter : IDetailedTemporaryFileWriter, ITemporaryFileWriter, IFileWriter
    {
        /// <inheritdoc />
        public bool IsWritingEnabled => true;


        /// <inheritdoc />
        public void CreateDirectory(string path)
        {
        }

        /// <inheritdoc />
        public Task WriteToFileAsync(string filepath, string text)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Stream OpenStreamForWriting(string filepath)
        {
            return new MemoryStream();
        }

        /// <inheritdoc />
        public Stream CreateFile(string path)
        {
            return new MemoryStream();
        }

        /// <inheritdoc />
        public Task WriteStreamAsync(string path, MemoryStream stream)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void DeleteDirectory(string folder)
        {
        }
    }
}
