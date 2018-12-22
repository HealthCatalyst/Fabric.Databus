// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EntityJsonWriterUnitTests.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the EntityJsonWriterUnitTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Json.Tests
{
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The entity JSON writer unit tests.
    /// </summary>
    [TestClass]
    public class EntityJsonWriterUnitTests
    {
        /// <summary>
        /// The can write to stream async successfully.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [TestMethod]
        public async Task CanWriteToStreamAsyncSuccessfully()
        {
            var text = @"{""foo"":""good"" }";
            var document = JToken.Parse(text);

            using (var stream = new MemoryStream())
            {
                await new EntityJsonWriter().WriteToStreamAsync(document, stream);

                stream.Seek(0, SeekOrigin.Begin);

                using (var reader = new BinaryReader(
                    stream,
                    Encoding.UTF8,
                    true))
                {
                    var buffer = reader.ReadBytes(14);

                    Assert.AreEqual(14, buffer.Length);
                    Assert.AreEqual((int)'{', buffer[0]);
                }
            }
        }
    }
}
