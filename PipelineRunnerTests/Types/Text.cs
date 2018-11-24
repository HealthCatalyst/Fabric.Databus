// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FastMergeUnitTests.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the FastMergeUnitTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable InconsistentNaming
namespace PipelineRunnerTests.Types
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The output.
    /// </summary>
    public class Text
    {
        /// <summary>
        /// Gets or sets the root.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
        // ReSharper disable once InconsistentNaming
        public string root { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
        // ReSharper disable once InconsistentNaming
        public string data { get; set; }

        /// <summary>
        /// Gets or sets the patient.
        /// </summary>
        public Patient Patient { get; set; }

        /// <summary>
        /// Gets or sets the visit.
        /// </summary>
        public Visit Visit { get; set; }
    }
}
