// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MyConfigValidator.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the MyConfigValidator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.API.Wrappers
{
    using System.Threading.Tasks;

    using Fabric.Databus.Config;
    using Fabric.Databus.Domain.ConfigValidators;
    using Fabric.Databus.PipelineRunner;
    using Fabric.Databus.Shared;
    using Fabric.Databus.SqlGenerator;

    using Serilog;

    /// <inheritdoc />
    public class MyConfigValidator : IConfigValidator
    {
        /// <inheritdoc />
        public Task<ConfigValidationResult> ValidateFromTextAsync(string fileContents, ILogger logger)
        {
            var configValidator = CreateConfigValidator();
            return configValidator.ValidateFromTextAsync(fileContents, logger);
        }

        /// <inheritdoc />
        public void ValidateJob(IJob job, ILogger logger)
        {
            var configValidator = CreateConfigValidator();
            configValidator.ValidateJob(job, logger);
        }

        /// <inheritdoc />
        public async Task ValidateDataSourcesAsync(IJob job, ILogger logger)
        {
            var configValidator = CreateConfigValidator();
            await configValidator.ValidateDataSourcesAsync(job, logger);
        }

        /// <summary>
        /// The create config validator.
        /// </summary>
        /// <returns>
        /// The <see cref="IConfigValidator"/>.
        /// </returns>
        private static IConfigValidator CreateConfigValidator()
        {
            var sqlConnectionFactory = new ReliableSqlConnectionFactory();
            var sqlGeneratorFactory = new SqlGeneratorFactory();

            return new ConfigValidator(sqlConnectionFactory, sqlGeneratorFactory);
        }
    }
}