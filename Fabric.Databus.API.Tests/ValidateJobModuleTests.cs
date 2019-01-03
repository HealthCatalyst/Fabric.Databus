// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValidateJobModuleTests.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ValidateJobModuleTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.API.Tests
{
    using Fabric.Databus.API.Configuration;
    using Fabric.Databus.API.Modules;
    using Fabric.Databus.API.Wrappers;
    using Fabric.Databus.Domain.Jobs;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Nancy.Testing;

    using Serilog;

    /// <summary>
    /// The validate job module tests.
    /// </summary>
    [TestClass]
    public class ValidateJobModuleTests
    {
        /// <summary>
        /// The can validate job.
        /// </summary>
        [TestMethod]
        public void CanValidateJob()
        {
            var logger = new LoggerConfiguration().CreateLogger();
            var jobScheduler = new JobScheduler(
                logger,
                new InMemoryJobHistoryStore(),
                new JobStatusTrackerFactory(),
                new MyPipelineRunner(),
                new MyConfigValidator());

            var appConfiguration = new AppConfiguration();

            var browser = new Browser(
                with =>
                    {
                        with.Module(
                            new ValidateJobModule(logger, jobScheduler, appConfiguration));
                    });

            var response = browser.Post(
                "/",
                (with) =>
                    {
                        with.HttpRequest();
                    });
        }
    }
}
