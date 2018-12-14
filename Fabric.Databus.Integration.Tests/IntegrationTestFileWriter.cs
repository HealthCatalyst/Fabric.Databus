// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IntegrationTestFileWriter.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IntegrationTestFileWriter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Integration.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces.FileWriters;

    /// <summary>
    /// The test file writer.
    /// </summary>
    public class IntegrationTestFileWriter : IFileWriter, ITemporaryFileWriter
    {
        /// <summary>
        /// The files.
        /// </summary>
        private readonly Dictionary<string, string> files = new Dictionary<string, string>();

        /// <inheritdoc />
        public bool IsWritingEnabled { get; set; }

        /// <summary>
        /// The count.
        /// </summary>
        public int Count => this.files.Count;

        /// <inheritdoc />
        public void CreateDirectory(string path)
        {
            // no need to do anything
        }

        /// <inheritdoc />
        public Task WriteToFileAsync(string filepath, string text)
        {
            if (!this.files.ContainsKey(filepath))
            {
                this.files.Add(filepath, text);
            }
            else
            {
                this.files[filepath] = text;
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Stream OpenStreamForWriting(string filepath)
        {
            return new IntegrationTestFileStream(this, filepath);
        }

        /// <inheritdoc />
        public Stream CreateFile(string path)
        {
            return this.OpenStreamForWriting(path);
        }

        /// <inheritdoc />
        public Task WriteStreamAsync(string path, Stream stream)
        {
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            using (var s = new StreamReader(stream))
            {
                var text = s.ReadToEnd();
                return this.WriteToFileAsync(path, text);
            }
        }

        /// <inheritdoc />
        public void DeleteDirectory(string folder)
        {
            // do nothing
        }

        /// <inheritdoc />
        public string CombinePath(string folder, string file)
        {
            return $"{folder}|{file}";
        }

        /// <summary>
        /// The contains file.
        /// </summary>
        /// <param name="expectedPath">
        /// The expected path.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool ContainsFile(string expectedPath)
        {
            return this.files.ContainsKey(expectedPath);
        }

        /// <summary>
        /// The get contents.
        /// </summary>
        /// <param name="expectedPath">
        /// The expected path.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetContents(string expectedPath)
        {
            return this.files[expectedPath];
        }

        /// <summary>
        /// The list files.
        /// </summary>
        /// <returns>
        /// The <see cref="List{T}"/>.
        /// </returns>
        public List<string> GetAllFileNames()
        {
            return this.files.Keys.ToList();
        }
    }
}