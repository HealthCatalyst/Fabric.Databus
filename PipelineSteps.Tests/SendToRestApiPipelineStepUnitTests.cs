﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SendToRestApiPipelineStepUnitTests.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SendToRestApiPipelineStepUnitTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PipelineStep.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using CreateBatchItemsPipelineStep;

    using Fabric.Databus.Config;
    using Fabric.Databus.Http;
    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Http;
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.Json;
    using Fabric.Databus.Shared.FileWriters;
    using Fabric.Databus.Shared.Queues;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;
    using Moq.Protected;

    using Newtonsoft.Json.Linq;

    using QueueItems;

    using SendToRestApiPipelineStep;

    using Serilog;

    /// <summary>
    /// The send to rest api pipeline step unit tests.
    /// </summary>
    [TestClass]
    public class SendToRestApiPipelineStepUnitTests
    {
        /// <summary>
        /// The http call is made.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [TestMethod]
        public async Task HttpCallIsMade()
        {
            // Arrange
            var job = new Job
            {
                Config = new QueryConfig
                {
                    LocalSaveFolder = Path.GetTempPath(),
                    EntitiesPerUploadFile = 1
                }
            };

            var queueManager = new QueueManager();

            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var mockRepository = new MockRepository(MockBehavior.Strict);

                var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
                                       {
                                           StatusCode = HttpStatusCode.NoContent,
                                           Content = new StringContent(string.Empty),
                                       };

                var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
                handlerMock
                    .Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(mockResponse)
                    .Verifiable();

                var mockHttpClientFactory = mockRepository.Create<IHttpClientFactory>();
                mockHttpClientFactory.Setup(service => service.Create())
                    .Returns(new HttpClient(handlerMock.Object));

                var hosts = new List<string> { "http://foo" };

                var fileUploader = new FileUploader(logger, hosts, mockHttpClientFactory.Object, "username", "password");

                IEntityJsonWriter entityJsonWriter = new EntityJsonWriter();

                var sendToRestApiPipelineStep = new SendToRestApiPipelineStep(
                    job.Config,
                    logger,
                    queueManager,
                    new MockProgressMonitor(),
                    fileUploader,
                    new NullFileWriter(), 
                    entityJsonWriter,
                    cancellationTokenSource.Token);

                var stepNumber = 1;
                queueManager.CreateInputQueue<IJsonObjectQueueItem>(stepNumber);

                sendToRestApiPipelineStep.CreateOutQueue(stepNumber);

                sendToRestApiPipelineStep.InitializeWithStepNumber(stepNumber);

                string queryId = "1";

                var jsonObjectQueueItem1 = new JsonObjectQueueItem
                {
                    Document = JObject.Parse(@"{test:'ff'}"),
                    PropertyName = "foo",
                    QueryId = queryId
                };

                await sendToRestApiPipelineStep.InternalHandleAsync(jsonObjectQueueItem1);

                // Assert
                var queues = queueManager.Queues;

                Assert.AreEqual(2, queues.Count);

                var meteredBlockingCollection = queues.First(queue => queue.Key == "EndPointQueueItem2").Value;
                var outputQueue = meteredBlockingCollection as IQueue<EndPointQueueItem>;
            }

        }
    }
}
