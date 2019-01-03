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
    using Fabric.Databus.Domain.ConfigValidators;
    using Fabric.Databus.Domain.Importers;
    using Fabric.Databus.Domain.Jobs;
    using Fabric.Databus.Shared;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

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

            var mockRepository = new MockRepository(MockBehavior.Strict);
            var mockConfigValidator = mockRepository.Create<IConfigValidator>();
            mockConfigValidator.Setup(service => service.ValidateFromTextAsync(fileContents, logger)).ReturnsAsync(
                new ConfigValidationResult { Success = true, ErrorText = string.Empty });

            var mockImportRunner = mockRepository.Create<IImportRunner>();

            var jobScheduler = new JobScheduler(
                logger,
                new InMemoryJobHistoryStore(),
                new JobStatusTrackerFactory(),
                mockImportRunner.Object,
                mockConfigValidator.Object);

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

            var validationResult = response.Body.DeserializeJson<ConfigValidationResult>();

            Assert.IsTrue(validationResult.Success, validationResult.ErrorText);
            Assert.AreEqual(string.Empty, validationResult.ErrorText);
        }

        /// <summary>
        /// The fails validate job async.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [TestMethod]
        public async Task FailsValidateJobAsync()
        {
            var fileContents = TestFileLoader.GetFileContents("Files", "badjob.xml");

            Assert.IsNotNull(fileContents);
            Assert.IsFalse(string.IsNullOrWhiteSpace(fileContents));

            var logger = new LoggerConfiguration().CreateLogger();

            var mockRepository = new MockRepository(MockBehavior.Strict);
            var mockConfigValidator = mockRepository.Create<IConfigValidator>();
            mockConfigValidator.Setup(service => service.ValidateFromTextAsync(fileContents, logger)).ReturnsAsync(
                new ConfigValidationResult { Success = false, ErrorText = "I am an error" });

            var mockImportRunner = mockRepository.Create<IImportRunner>();

            var jobScheduler = new JobScheduler(
                logger,
                new InMemoryJobHistoryStore(),
                new JobStatusTrackerFactory(),
                mockImportRunner.Object,
                mockConfigValidator.Object);

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

            var validationResult = response.Body.DeserializeJson<ConfigValidationResult>();

            Assert.IsFalse(validationResult.Success);
            Assert.AreNotEqual(string.Empty, validationResult.ErrorText);
        }
    }
}
