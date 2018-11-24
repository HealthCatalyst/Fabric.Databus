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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The visit.
    /// </summary>
    public class Visit
    {
        /// <summary>
        /// Gets or sets the root.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
        public string root { get; set; }

        /// <summary>
        /// Gets or sets the people.
        /// </summary>
        // ReSharper disable once MemberHidesStaticFromOuterClass
        public IList<People> People { get; set; }
    }
}
