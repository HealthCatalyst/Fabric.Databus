// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigValidator.cs" company="">
//   
// </copyright>
// <summary>
//   The config validator.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.PipelineRunner
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;

    using Fabric.Databus.Config;
    using Fabric.Databus.Domain.ConfigValidators;
    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.Sql;
    using Fabric.Databus.Shared;

    using Serilog;

    /// <inheritdoc />
    public class ConfigValidator : IConfigValidator
    {
        /// <summary>
        /// The sql connection factory.
        /// </summary>
        private readonly ISqlConnectionFactory sqlConnectionFactory;

        /// <summary>
        /// The sql generator factory.
        /// </summary>
        private readonly ISqlGeneratorFactory sqlGeneratorFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigValidator"/> class.
        /// </summary>
        /// <param name="sqlConnectionFactory">
        /// The sql connection factory.
        /// </param>
        /// <param name="sqlGeneratorFactory">
        /// The sql Generator Factory.
        /// </param>
        public ConfigValidator(ISqlConnectionFactory sqlConnectionFactory, ISqlGeneratorFactory sqlGeneratorFactory)
        {
            this.sqlConnectionFactory = sqlConnectionFactory;
            this.sqlGeneratorFactory = sqlGeneratorFactory;
        }

        /// <inheritdoc />
        /// <summary>
        /// The validate from text.
        /// </summary>
        /// <param name="fileContents">
        /// The file contents.
        /// </param>
        /// <param name="logger"></param>
        /// <returns>
        /// The <see cref="T:System.Threading.Tasks.Task" />.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">exception thrown
        /// </exception>
        public async Task<ConfigValidationResult> ValidateFromTextAsync(string fileContents, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(fileContents))
            {
                throw new ArgumentNullException(nameof(fileContents));
            }

            var configValidationResult = new ConfigValidationResult
            {
                Success = true,
                Results = new List<string>(),
            };

            try
            {
                var job = new ConfigReader().ReadXmlFromText(fileContents);

                configValidationResult.Results.Add("Config format: OK");

                //var ping = new Ping();

                //PingReply pingResult = null;
                //ping.SendPingAsync("dockerhost").ContinueWith((a) => pingResult = a.Result).Wait();

                //configValidationResult.Results.Add($"ping dockerhost: {pingResult?.Address}");

                configValidationResult.Results.Add($"Sql Connection String: {job.Config.ConnectionString}");

                await this.CheckDatabaseConnectionStringIsValid(job);

                configValidationResult.Results.Add($"Sql Connection String: OK");

                var firstQueryIsValid = await this.CheckFirstQueryIsValid(job, logger) ? "OK" : "No Rows";

                configValidationResult.Results.Add($"First Query: {firstQueryIsValid}");

                int i = 0;
                foreach (var load in job.Data.DataSources)
                {
                    var queryIsValid = await this.CheckQueryIsValid(job, load, logger, ++i) ? "OK" : "No Rows";

                    configValidationResult.Results.Add($"Query [{load.Path}]: {queryIsValid}");
                }

                if (job.Config.UploadToUrl)
                {
                    // string x = await (new ElasticSearchUploader(job.Config.UrlUserName,
                    //    job.Config.UrlPassword, job.Config.KeepIndexOnline).TestElasticSearchConnection(job.Config.Urls));

                    // configValidationResult.Results.Add($"ElasticSearch Connection: {x}");
                }

            }
            catch (Exception ex)
            {
                configValidationResult.ErrorText = ex.ToString();
                configValidationResult.Success = false;
            }

            return configValidationResult;
        }

        /// <summary>
        /// The check database connection string is valid.
        /// </summary>
        /// <param name="job">
        /// The job.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public Task<bool> CheckDatabaseConnectionStringIsValid(IJob job)
        {
            if (string.IsNullOrWhiteSpace(job.Config.ConnectionString))
            {
                throw new Exception("Connection String is empty or null");
            }

            using (var conn = this.sqlConnectionFactory.GetConnection(job.Config.ConnectionString))
            {
                conn.Open();
                conn.Close();
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// The check first query is valid.
        /// </summary>
        /// <param name="job">
        ///     The job.
        /// </param>
        /// <param name="logger">
        /// The logger
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public async Task<bool> CheckFirstQueryIsValid(IJob job, ILogger logger)
        {
            var load = job.Data.DataSources.First(c => c.Path == null);

            return await this.CheckQueryIsValid(job, load, logger, 1);
        }

        /// <summary>
        /// The check query is valid.
        /// </summary>
        /// <param name="job">
        ///     The job.
        /// </param>
        /// <param name="load">
        ///     The load.
        /// </param>
        /// <param name="logger">
        ///     the logger
        /// </param>
        /// <param name="numberOfDataSource">
        /// number of data source
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="Exception">exception thrown
        /// </exception>
        public async Task<bool> CheckQueryIsValid(IJob job, IDataSource load, ILogger logger, int numberOfDataSource)
        {
            logger.Information("Validating data source {index} {path} {@load} {@StartTime}", numberOfDataSource, load.Path, load, DateTime.Now);

            using (var conn = this.sqlConnectionFactory.GetConnection(job.Config.ConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();

                if (job.Config.SqlCommandTimeoutInSeconds != 0)
                {
                    cmd.CommandTimeout = job.Config.SqlCommandTimeoutInSeconds;
                }

                cmd.CommandText = this.sqlGeneratorFactory.Create().AddCTE(load.Sql).AddTopFilter(0).ToSqlString();

                var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

                var foundRow = false;

                while (reader.Read())
                {
                    foundRow = true;
                }

                logger.Information("Validated data source {index} {path} {@load} {@StartTime}", numberOfDataSource, load.Path, load, DateTime.Now);

                return foundRow;
            }
        }

        /// <inheritdoc />
        public void ValidateJob(IJob job, ILogger logger)
        {
            // logger.Information("Validating Job {@Job}", job);
             logger.Information("Validating job {@StartTime}", DateTime.Now);
            if (job == null)
            {
                throw new Exception("job cannot be null");
            }

            if (job.Config == null)
            {
                throw new Exception("job.Config cannot be null");
            }

            if (string.IsNullOrWhiteSpace(job.Config.ConnectionString))
            {
                throw new Exception("No connection string was passed");
            }

            if (!this.CheckDatabaseConnectionStringIsValid(job).Result)
            {
                throw new Exception($"Unable to connect to connection string: [{job.Config.ConnectionString}]");
            }

            if (job.Data == null)
            {
                throw new Exception("job.Data cannot be null");
            }

            if (job.Data.TopLevelDataSource == null)
            {
                throw new Exception("No TopLevelDataSource was specified");
            }

            if (string.IsNullOrWhiteSpace(job.Data.TopLevelDataSource.Key))
            {
                throw new Exception("No Key was specified in TopLevelDataSource");
            }

            if (!job.Data.DataSources.Any())
            {
                throw new Exception("No data sources were specified");
            }

            int i = 0;
            foreach (var dataSource in job.Data.DataSources)
            {
                i++;
                if (string.IsNullOrWhiteSpace(dataSource.Sql) && string.IsNullOrWhiteSpace(dataSource.TableOrView))
                {
                    throw new Exception(
                        $"Both Sql and TableOrView is empty for dataSource index {i} with name {dataSource.Name} and path {dataSource.Path}");
                }

                if (dataSource.Relationships == null)
                {
                    throw new Exception("dataSource.Relationships cannot be null");
                }

                if (dataSource.SqlEntityColumnMappings == null)
                {
                    throw new Exception("dataSource.SqlEntityColumnMappings cannot be null");
                }
            }
            logger.Information("Validated job {@StartTime}", DateTime.Now);
        }

        /// <inheritdoc />
        public void ValidateDataSources(IJob job, ILogger logger)
        {
            int numberOfDataSource = 0;
            foreach (var dataSource in job.Data.DataSources)
            {
                numberOfDataSource++;
                try
                {
                    this.CheckQueryIsValid(job, dataSource, logger, numberOfDataSource).Wait();
                }
                catch (Exception e)
                {
                    throw new Exception(
                        $"Error in dataSource index {numberOfDataSource} with name [{dataSource.Name}] and path [{dataSource.Path}] and Sql [{dataSource.Sql}]",
                        e);
                }
            }
        }
    }
}