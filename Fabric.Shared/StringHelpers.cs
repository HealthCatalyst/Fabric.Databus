// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringHelpers.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the StringHelpers type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Shared
{
    using System;
    using System.Globalization;

    /// <summary>
    /// The string helpers.
    /// </summary>
    public static class StringHelpers
    {
        /// <summary>
        /// The bytes to string.
        /// </summary>
        /// <param name="byteCount">
        /// The byte count.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ToDisplayString(this long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; // Longs run out around EB
            if (byteCount == 0)
            {
                return $"0 {suf[0]}";
            }

            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            var numberText = (Math.Sign(byteCount) * num).ToString(CultureInfo.InvariantCulture);
            return $"{numberText,4} {suf[place]}";
        }
    }
}
