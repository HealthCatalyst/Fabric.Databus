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
    /// The people.
    /// </summary>
    public class People
    {
        /// <summary>
        /// Gets or sets the root.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
        public string root { get; set; }

        /// <summary>
        /// Gets or sets the last_name.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
        public string last_name { get; set; }
    }
}
