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
        /// <returns>
        /// The <see cref="T:System.Threading.Tasks.Task" />.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">exception thrown
        /// </exception>
        public async Task<ConfigValidationResult> ValidateFromTextAsync(string fileContents)
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

                var firstQueryIsValid = await this.CheckFirstQueryIsValid(job) ? "OK" : "No Rows";

                configValidationResult.Results.Add($"First Query: {firstQueryIsValid}");

                foreach (var load in job.Data.DataSources)
                {
                    var queryIsValid = await this.CheckQueryIsValid(job, load) ? "OK" : "No Rows";

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
        /// The job.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public async Task<bool> CheckFirstQueryIsValid(IJob job)
        {
            var load = job.Data.DataSources.First(c => c.Path == null);

            return await this.CheckQueryIsValid(job, load);
        }

        /// <summary>
        /// The check query is valid.
        /// </summary>
        /// <param name="job">
        /// The job.
        /// </param>
        /// <param name="load">
        /// The load.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="Exception">exception thrown
        /// </exception>
        public async Task<bool> CheckQueryIsValid(IJob job, IDataSource load)
        {
            var numberOfLevels =
                (load.Path?.Count(a => a == '.') + 1) ?? 0;

            using (var conn = this.sqlConnectionFactory.GetConnection(job.Config.ConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();

                if (job.Config.SqlCommandTimeoutInSeconds != 0)
                {
                    cmd.CommandTimeout = job.Config.SqlCommandTimeoutInSeconds;
                }

                cmd.CommandText = this.sqlGeneratorFactory.Create().AddCTE(load.Sql).AddTopFilter(1).ToSqlString();

                var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

                var foundRow = false;

                while (reader.Read())
                {
                    foundRow = true;
                }

                return foundRow;
            }
        }

        /// <inheritdoc />
        public void ValidateJob(IJob job)
        {
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

            if (string.IsNullOrWhiteSpace(job.Config.TopLevelKeyColumn))
            {
                throw new Exception("No TopLevelKeyColumn was specified");
            }

            if (job.Data == null)
            {
                throw new Exception("job.Data cannot be null");
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
        }

        /// <inheritdoc />
        public void ValidateDataSources(IJob job)
        {
            int i = 0;
            foreach (var dataSource in job.Data.DataSources)
            {
                i++;
                try
                {
                    this.CheckQueryIsValid(job, dataSource).Wait();
                }
                catch (Exception e)
                {
                    throw new Exception(
                        $"Error in dataSource index {i} with name [{dataSource.Name}] and path [{dataSource.Path}] and Sql [{dataSource.Sql}]",
                        e);
                }
            }
        }
    }
}