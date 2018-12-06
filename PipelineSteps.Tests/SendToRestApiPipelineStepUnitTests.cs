// --------------------------------------------------------------------------------------------------------------------
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
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Config;
    using Fabric.Databus.Http;
    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Http;
    using Fabric.Databus.Interfaces.Queues;
    using Fabric.Databus.Json;
    using Fabric.Databus.PipelineSteps;
    using Fabric.Databus.QueueItems;
    using Fabric.Databus.Shared.FileWriters;
    using Fabric.Databus.Shared.Queues;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;
    using Moq.Protected;

    using Newtonsoft.Json.Linq;

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

            var queueManager = new QueueManager(new SingleThreadedQueueFactory());

            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var mockRepository = new MockRepository(MockBehavior.Strict);

                string queryId = "1";

                var jsonObjectQueueItem1 = new JsonObjectQueueItem
                                               {
                                                   Document = JObject.Parse(@"{test:'ff'}"),
                                                   PropertyName = "foo",
                                                   QueryId = queryId
                                               };

                var expectedRequestMessageContent = "{\"test\":\"ff\"}";

                var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
                handlerMock
                    .Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .Callback<HttpRequestMessage, CancellationToken>((request, token) =>
                        {
                            var result = request.Content.ReadAsStringAsync().Result;
                            Assert.AreEqual(expectedRequestMessageContent, result);
                        })
                    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(string.Empty)
                    })
                    .Verifiable();

                var fullUri = new Uri("http://foo");

                var mockHttpClientFactory = mockRepository.Create<IHttpClientFactory>();
                mockHttpClientFactory.Setup(service => service.Create())
                    .Returns(new HttpClient(handlerMock.Object));

                var hosts = new List<string> { fullUri.ToString() };

                var mockHttpRequestInjector = mockRepository.Create<IHttpRequestInterceptor>();
                mockHttpRequestInjector.Setup(
                    service => service.InterceptRequest(It.IsAny<HttpMethod>(), It.IsAny<HttpRequestMessage>()));

                var mockHttpResponseInjector = mockRepository.Create<IHttpResponseInterceptor>();
                mockHttpResponseInjector.Setup(
                    service => service.InterceptResponse(HttpMethod.Put, It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<HttpStatusCode>(), It.IsAny<string>(), It.IsAny<long>()));

                var fileUploader = new FileUploader(
                    logger,
                    hosts,
                    mockHttpClientFactory.Object,
                    mockHttpRequestInjector.Object,
                    mockHttpResponseInjector.Object,
                    HttpMethod.Put);

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

                await sendToRestApiPipelineStep.InternalHandleAsync(jsonObjectQueueItem1);

                // Assert
                var queues = queueManager.Queues;

                Assert.AreEqual(2, queues.Count);

                var meteredBlockingCollection = queues.First(queue => queue.Key == nameof(EndPointQueueItem) + "2").Value;
                var outputQueue = meteredBlockingCollection as IQueue<EndPointQueueItem>;

                handlerMock.Protected()
                    .Verify(
                        "SendAsync",
                        Times.Exactly(1),
                        ItExpr.Is<HttpRequestMessage>(
                            req => req.Method == HttpMethod.Put
                                   && req.RequestUri == fullUri),
                        ItExpr.IsAny<CancellationToken>());
            }
        }
    }
}
