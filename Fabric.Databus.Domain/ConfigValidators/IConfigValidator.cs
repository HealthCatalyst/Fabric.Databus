// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IConfigValidator.cs" company="">
//   
// </copyright>
// <summary>
//   The ConfigValidator interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Domain.ConfigValidators
{
    using System.Threading.Tasks;

    using Fabric.Databus.Config;
    using Fabric.Databus.Shared;

    /// <summary>
    /// The ConfigValidator interface.
    /// </summary>
    public interface IConfigValidator
    {
        /// <summary>
        /// The validate from text async.
        /// </summary>
        /// <param name="fileContents">
        /// The file contents.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task<ConfigValidationResult> ValidateFromTextAsync(string fileContents);

        /// <summary>
        /// The validate job.
        /// </summary>
        /// <param name="job">
        /// The job.
        /// </param>
        void ValidateJob(IJob job);

        /// <summary>
        /// The validate data sources.
        /// </summary>
        /// <param name="job">
        /// The job.
        /// </param>
        void ValidateDataSources(IJob job);
    }
}
