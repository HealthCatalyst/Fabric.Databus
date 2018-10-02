// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFileWriter.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IFileWriter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces
{
    using System;
    using System.IO;

    /// <summary>
    /// The FileWriter interface.
    /// </summary>
    public interface IFileWriter
    {
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
        void WriteToFile(string filepath, string text);

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
        /// The path.
        /// </param>
        /// <param name="stream">
        /// The stream.
        /// </param>
        void WriteStream(string path, MemoryStream stream);

        /// <summary>
        /// The delete directory.
        /// </summary>
        /// <param name="folder">
        /// The folder.
        /// </param>
        /// <exception cref="Exception">exception thrown
        /// </exception>
        void DeleteDirectory(string folder);
    }
}
