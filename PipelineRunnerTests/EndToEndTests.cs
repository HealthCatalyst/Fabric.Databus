// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EndToEndTests.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the EndToEndTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PipelineRunnerTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;

    using ElasticSearchSqlFeeder.Shared;

    using Fabric.Databus.Config;
    using Fabric.Databus.Domain.ProgressMonitors;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using PipelineRunner;

    /// <summary>
    /// The end to end tests.
    /// </summary>
    [TestClass]
    public class EndToEndTests
    {
        /// <summary>
        /// The test elastic search pipeline.
        /// </summary>
        [TestMethod]
        public void TestElasticSearchPipeline()
        {
            var job = new Job
            {
                Config = new QueryConfig { LocalSaveFolder = Path.GetTempPath() },
                Data = new JobData
                {
                    DataSources = new List<DataSource>
                                    {
                                       new DataSource
                                           {
                                               Sql = @"SELECT
                                              CustomerNM
                                              ,CustomerID
                                              ,	AliasPatientID
                                              ,	GenderNormDSC
                                              ,RaceNormDSC
                                              ,MaritalStatusNormDSC  
                                              FROM CAFEEDW.SharedClinicalUnion.ElasticsearchInputPatient where CustomerID = 4"
                                           }
                                    }
                }
            };

            using (ProgressMonitor progressMonitor = new ProgressMonitor(new StringProgressLogger()))
            {
                using (var cancellationTokenSource = new CancellationTokenSource())
                {
                    var pipelineRunner = new PipelineRunner();  
                    pipelineRunner.Init();
                    try
                    {
                        pipelineRunner.RunPipeline(job, progressMonitor, cancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException e)
                    {
                        Console.WriteLine(e.ToString());
                        throw;
                    }
                    catch (AggregateException e)
                    {
                        Console.WriteLine(e.Flatten().ToString());
                        throw;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }
        }
    }
}
