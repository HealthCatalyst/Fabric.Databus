// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileWriter.cs" company="">
//   
// </copyright>
// <summary>
//   The file writer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ElasticSearchSqlFeeder.Shared
{
    using System;
    using System.IO;

    using ElasticSearchSqlFeeder.Interfaces;

    /// <inheritdoc />
    /// <summary>
    /// The file writer.
    /// </summary>
    public class FileWriter : IFileWriter
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


        /// <summary>
        /// The delete directory.
        /// </summary>
        /// <param name="folder">
        /// The folder.
        /// </param>
        /// <exception cref="Exception">exception thrown
        /// </exception>
        public void DeleteDirectory(string folder)
        {
            if (!Directory.Exists(folder))
            {
                throw new Exception($"Folder {folder} does not exist");
            }

            string[] files = Directory.GetFiles(folder);
            string[] dirs = Directory.GetDirectories(folder);

            foreach (var file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                this.DeleteDirectory(dir);
            }
        }
    }
}
