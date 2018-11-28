// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IntegrationTestFileStream.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IntegrationTestFileStream type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Integration.Tests
{
    using System.IO;

    /// <inheritdoc />
    /// <summary>
    /// The integration test file stream.
    /// </summary>
    public class IntegrationTestFileStream : Stream
    {
        /// <summary>
        /// The writer.
        /// </summary>
        private readonly IntegrationTestFileWriter writer;

        /// <summary>
        /// The filepath.
        /// </summary>
        private readonly string filepath;

        /// <summary>
        /// The stream implementation.
        /// </summary>
        private readonly Stream streamImplementation;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationTestFileStream"/> class.
        /// </summary>
        /// <param name="writer">
        /// The writer.
        /// </param>
        /// <param name="filepath">
        /// file path</param>
        public IntegrationTestFileStream(IntegrationTestFileWriter writer, string filepath)
        {
            this.writer = writer;
            this.filepath = filepath;
            this.streamImplementation = new MemoryStream();
        }

        /// <inheritdoc />
        public override bool CanRead => this.streamImplementation.CanRead;

        /// <inheritdoc />
        public override bool CanSeek => this.streamImplementation.CanSeek;

        /// <inheritdoc />
        public override bool CanWrite => this.streamImplementation.CanWrite;

        /// <inheritdoc />
        public override long Length => this.streamImplementation.Length;

        /// <inheritdoc />
        public override long Position
        {
            get => this.streamImplementation.Position;
            set => this.streamImplementation.Position = value;
        }

        /// <inheritdoc />
        public override void Flush()
        {
            this.streamImplementation.Flush();
            this.writer.WriteStreamAsync(this.filepath, this.streamImplementation);
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.streamImplementation.Seek(offset, origin);
        }

        /// <inheritdoc />
        public override void SetLength(long value)
        {
            this.streamImplementation.SetLength(value);
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.streamImplementation.Read(buffer, offset, count);
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            this.streamImplementation.Write(buffer, offset, count);
        }
    }
}