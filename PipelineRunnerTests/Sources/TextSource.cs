// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FastMergeUnitTests.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the FastMergeUnitTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable InconsistentNaming
namespace PipelineRunnerTests.Sources
{
    /// <summary>
    /// The text.
    /// </summary>
    public class TextSource
    {
        /// <summary>
        /// Gets or sets the text id.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string TextID { get; set; }

        /// <summary>
        /// Gets or sets the text txt.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string TextTXT { get; set; }

        /// <summary>
        /// Gets or sets the edw patient id.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string EDWPatientId { get; set; }

        /// <summary>
        /// Gets or sets the encounter id.
        /// </summary>
        public string EncounterID { get; set; }
    }
}
