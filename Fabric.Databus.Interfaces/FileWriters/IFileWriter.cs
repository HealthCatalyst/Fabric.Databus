// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFileWriter.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IFileWriter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.FileWriters
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// The FileWriter interface.
    /// </summary>
    public interface IFileWriter
    {
        /// <summary>
        /// Gets a value indicating whether is writing enable.
        /// </summary>
        bool IsWritingEnabled { get; }

        /// <summary>
        /// The create directory.
        /// </summary>
        /// <param name="path">
        /// The path.
        /// </param>
        void CreateDirectory(string path);

        /// <summary>
        /// The write to file.
        /// </summary>
        /// <param name="filepath">
        /// The filepath.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task WriteToFileAsync(string filepath, string text);

        /// <summary>
        /// The open stream for writing.
        /// </summary>
        /// <param name="filepath">
        /// The filepath.
        /// </param>
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        Stream OpenStreamForWriting(string filepath);

        /// <summary>
        /// The create file.
        /// </summary>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        Stream CreateFile(string path);

        /// <summary>
        /// The write stream.
        /// </summary>
        /// <param name="path">
        ///     The path.
        /// </param>
        /// <param name="stream">
        ///     The stream.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task WriteStreamAsync(string path, Stream stream);

        /// <summary>
        /// The delete directory.
        /// </summary>
        /// <param name="folder">
        /// The folder.
        /// </param>
        /// <exception cref="Exception">exception thrown
        /// </exception>
        void DeleteDirectory(string folder);

        /// <summary>
        /// The combine path.
        /// </summary>
        /// <param name="folder">
        /// The folder.
        /// </param>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string CombinePath(string folder, string file);
    }
}
