// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestFileLoader.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the TestFileLoader type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Database.Testing.FileLoader
{
    using System.Collections.Generic;
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
        public static string GetFileContents(string folder, string sampleFile)
        {
            var asm = Assembly.GetCallingAssembly();
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

        /// <summary>
        /// The get file contents as list.
        /// </summary>
        /// <param name="folder">
        /// The folder.
        /// </param>
        /// <param name="sampleFile">
        /// The sample file.
        /// </param>
        /// <returns>
        /// The <see cref="List{T}"/>.
        /// </returns>
        [Pure]
        public static List<string> GetFileContentsAsList(string folder, string sampleFile)
        {
            var asm = Assembly.GetCallingAssembly();
            var assemblyName = asm.GetName().Name;
            var resource = $"{assemblyName}.{folder}.{sampleFile}";

            var list = new List<string>();

            using (var stream = asm.GetManifestResourceStream(resource))
            {
                if (stream != null)
                {
                    var reader = new StreamReader(stream);
                    while (reader.Peek() >= 0)
                    {
                        list.Add(reader.ReadLine());
                    }
                }
            }

            return list;
        }
    }
}