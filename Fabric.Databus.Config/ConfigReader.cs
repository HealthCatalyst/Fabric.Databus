// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigReader.cs" company="">
//   
// </copyright>
// <summary>
//   The config reader.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Config
{
    using System.IO;

    using Fabric.Shared;

    /// <summary>
    /// The config reader.
    /// </summary>
    public class ConfigReader
    {
        /// <summary>
        /// The read xml.
        /// </summary>
        /// <param name="inputFile">
        /// The input file.
        /// </param>
        /// <returns>
        /// The <see cref="XmlJob"/>.
        /// </returns>
        public XmlJob ReadXml(string inputFile)
        {
            var fileContents = File.ReadAllText(inputFile);

            return this.ReadXmlFromText(fileContents);
        }

        /// <summary>
        /// The read xml from text.
        /// </summary>
        /// <param name="fileContents">
        /// The file contents.
        /// </param>
        /// <returns>
        /// The <see cref="XmlJob"/>.
        /// </returns>
        public XmlJob ReadXmlFromText(string fileContents)
        {
            return fileContents.FromXml<XmlJob>();
        }
    }
}
