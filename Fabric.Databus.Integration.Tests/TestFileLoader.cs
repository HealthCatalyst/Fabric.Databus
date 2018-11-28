// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestFileLoader.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the TestFileLoader type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Integration.Tests
{
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// The test file loader.
    /// </summary>
    public static class TestFileLoader
    {
        /// <summary>
        /// The get file contents.
        /// </summary>
        /// <param name="folder">
        /// The folder.
        /// </param>
        /// <param name="sampleFile">
        /// The sample file.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        [Pure]
        internal static string GetFileContents(string folder, string sampleFile)
        {
            var asm = Assembly.GetExecutingAssembly();
            var assemblyName = asm.GetName().Name;
            var resource = $"{assemblyName}.{folder}.{sampleFile}";
            using (var stream = asm.GetManifestResourceStream(resource))
            {
                if (stream != null)
                {
                    var reader = new StreamReader(stream);
                    return reader.ReadToEnd();
                }
            }
            return string.Empty;
        }
    }
}