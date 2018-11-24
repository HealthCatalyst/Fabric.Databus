﻿// --------------------------------------------------------------------------------------------------------------------
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
    /// The output patient.
    /// </summary>
    public class Patient
    {
        /// <summary>
        /// Gets or sets the root.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
        // ReSharper disable once InconsistentNaming
        public string root { get; set; }

        /// <summary>
        /// Gets or sets the mrn.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string MRN { get; set; }
    }
}
