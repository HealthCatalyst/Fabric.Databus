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
    using System.Threading.Tasks;

    using Fabric.Database.Testing.FileLoader;
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
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [TestMethod]
        public async Task CanValidateJobAsync()
        {
            var fileContents = TestFileLoader.GetFileContents("Files", "job.xml");

            Assert.IsNotNull(fileContents);
            Assert.IsFalse(string.IsNullOrWhiteSpace(fileContents));

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

            var response = await browser.Post(
                "/validate",
                (with) =>
                    {
                        with.HttpRequest();
                        with.Body(fileContents);
                        with.Header("Accept", "application/json");
                    });

            Assert.AreEqual(Nancy.HttpStatusCode.OK, response.StatusCode);
        }
    }
}
