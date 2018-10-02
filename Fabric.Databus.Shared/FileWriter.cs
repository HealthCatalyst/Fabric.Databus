// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileWriter.cs" company="">
//   
// </copyright>
// <summary>
//   The file writer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared
{
    using System;
    using System.IO;

    using Fabric.Databus.Interfaces;

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
        public void WriteToFile(string filepath, string text)
        {
            File.WriteAllText(filepath, text);
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
            return File.Create(path);
        }

        /// <inheritdoc />
        public void WriteStream(string path, MemoryStream stream)
        {
            using (var fileStream = File.Create(path))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);

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
            if (!Directory.Exists(folder))
            {
                throw new Exception($"Folder {folder} does not exist");
            }

            Directory.Delete(folder, true);

            Directory.CreateDirectory(folder);
        }
    }
}
