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

    using Serilog;

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
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task<ConfigValidationResult> ValidateFromTextAsync(string fileContents, ILogger logger);

        /// <summary>
        /// The validate job.
        /// </summary>
        /// <param name="job">
        /// The job.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        void ValidateJob(IJob job, ILogger logger);

        /// <summary>
        /// The validate data sources.
        /// </summary>
        /// <param name="job">
        /// The job.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        void ValidateDataSources(IJob job, ILogger logger);
    }
}
