// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NullFileWriter.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the NullFileWriter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared
{
    using System.IO;

    using Fabric.Databus.Interfaces;

    /// <summary>
    /// The null file writer.
    /// </summary>
    public class NullFileWriter : IDetailedTemporaryFileWriter, ITemporaryFileWriter, IFileWriter
    {
        /// <inheritdoc />
        public void CreateDirectory(string path)
        {
        }

        /// <inheritdoc />
        public void WriteToFile(string filepath, string text)
        {
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
        public void WriteStream(string path, MemoryStream stream)
        {
        }

        /// <inheritdoc />
        public void DeleteDirectory(string folder)
        {
        }
    }
}
