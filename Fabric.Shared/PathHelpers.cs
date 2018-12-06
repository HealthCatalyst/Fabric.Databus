// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PathHelpers.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the PathHelpers type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Shared
{
    using System.IO;

    /// <summary>
    /// The path helpers.
    /// </summary>
    public static class PathHelpers
    {
        /// <summary>
        /// The get safe filename.
        /// </summary>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetSafeFilename(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
