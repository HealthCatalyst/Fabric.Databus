// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigValidationResult.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ConfigValidationResult type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared
{
    using System.Collections.Generic;

    /// <summary>
    /// The config validation result.
    /// </summary>
    public class ConfigValidationResult
    {
        public bool Success { get; set; }
        public List<string> Results { get; set; }
        public string ErrorText { get; set; }
    }
}