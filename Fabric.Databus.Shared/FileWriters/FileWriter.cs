// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileWriter.cs" company="">
//   
// </copyright>
// <summary>
//   The file writer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared.FileWriters
{
    using System.IO;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.FileWriters;

    /// <inheritdoc />
    /// <summary>
    /// The file writer.
    /// </summary>
    public class FileWriter : IDetailedTemporaryFileWriter, ITemporaryFileWriter, IFileWriter
    {
        /// <inheritdoc />
        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        /// <inheritdoc />
        public async Task WriteToFileAsync(string filepath, string text)
        {
            using (FileStream target = File.Create(filepath))
            {
                using (StreamWriter writer = new StreamWriter(target))
                {
                    await writer.WriteAsync(text);
                }
            }
        }

        /// <inheritdoc />
        public Stream OpenStreamForWriting(string filepath)
        {
            var file = File.OpenWrite(filepath);

            return file;
        }

        /// <inheritdoc />
        public Stream CreateFile(string path)
        {
            var directoryName = Path.GetDirectoryName(path);
            if (!Directory.Exists(directoryName))
            {
                this.CreateDirectory(directoryName);
            }

            return File.Create(path);
        }

        /// <inheritdoc />
        public async Task WriteStreamAsync(string path, MemoryStream stream)
        {
            using (var fileStream = File.Create(path))
            {
                stream.Seek(0, SeekOrigin.Begin);
                await stream.CopyToAsync(fileStream);

                fileStream.Flush();
            }
        }


        /// <inheritdoc />
        /// <summary>
        /// The delete directory.
        /// </summary>
        /// <param name="folder">
        /// The folder.
        /// </param>
        /// <exception cref="T:System.Exception">exception thrown
        /// </exception>
        public void DeleteDirectory(string folder)
        {
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);

                Directory.CreateDirectory(folder);
            }
        }
    }
}
