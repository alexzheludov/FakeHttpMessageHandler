using FakeHttpMessageHandler.Tests.Fakes;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FakeHttpMessageHandler.Tests
{
    [TestClass]
    public class FakeHttpMessageHandlerTest
    {
        private static readonly Uri _requestUri = new Uri("http://zheludov.com");


        [TestMethod]
        public async Task FakeHttpMessageHandler_Verify_ReturnsInstanceOfT()
        {
            var handler = new FakeHttpMessageHandler<FakeOutput>();
            using (var httpClient = new HttpClient(handler, true))
            {
                var responce = await httpClient.GetAsync(_requestUri);

                var responseText = await responce.Content.ReadAsStringAsync();
                var clientOutput = JsonConvert.DeserializeObject<FakeOutput>(responseText);

                responce.StatusCode.Should().Be(HttpStatusCode.OK);
                clientOutput.Should().NotBeNull();
                clientOutput.FakeProperty.Should().NotBeNullOrWhiteSpace();
                handler.CallsCount.Should().Be(1);
            }
        }

        [TestMethod]
        public async Task FakeHttpMessageHandler_Verify_ReturnsOutputOverride()
        {
            var output = new FakeOutput {
                FakeProperty = "MyFakeProperty"
            };

            var handler = new FakeHttpMessageHandler<FakeOutput>(output);


            using (var httpClient = new HttpClient(handler, true))
            {
                var responce = await httpClient.GetAsync(_requestUri);

                var responseText = await responce.Content.ReadAsStringAsync();
                var clientOutput = JsonConvert.DeserializeObject<FakeOutput>(responseText);

                clientOutput.Should().NotBeNull();
                clientOutput.Should().BeEquivalentTo(output);
                clientOutput.FakeProperty.Should().NotBeNullOrWhiteSpace();
                handler.CallsCount.Should().Be(1);
            }
        }

        [TestMethod]
        public async Task FakeHttpMessageHandler_Verify_ReturnStatusCodeOverride()
        {
            var output = new FakeOutput
            {
                FakeProperty = "MyFakeProperty"
            };

            var handler = new FakeHttpMessageHandler<FakeOutput>(output, httpStatusCode: HttpStatusCode.BadRequest);


            using (var httpClient = new HttpClient(handler, true))
            {
                var responce = await httpClient.GetAsync(_requestUri);
                responce.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                
                handler.CallsCount.Should().Be(1);
            }
        }

        [TestMethod]
        public async Task FakeHttpMessageHandler_Verify_ReturnsStringOutput()
        {
            var stringOutput = "FakeStringOutput";

            var handler = new FakeHttpMessageHandler<string>(stringOutput);
            
            using (var httpClient = new HttpClient(handler, true))
            {
                var responce = await httpClient.GetAsync(_requestUri);

                var responseText = await responce.Content.ReadAsStringAsync();
                responseText.Should().NotBeNull();
                responseText.Should().BeEquivalentTo(stringOutput);

                handler.CallsCount.Should().Be(1);
            }
        }

        [TestMethod]
        public async Task FakeHttpMessageHandler_Verify_ResetsCalls()
        {
            var handler = new FakeHttpMessageHandler<FakeOutput>();

            using (var httpClient = new HttpClient(handler, true))
            {
                var responce = await httpClient.GetAsync(_requestUri);

                var responseText = await responce.Content.ReadAsStringAsync();
                responseText.Should().NotBeNull();

                handler.CallsCount.Should().Be(1);
                handler.ResetCalls();
                handler.CallsCount.Should().Be(0);
            }
        }

        [TestMethod]
        public async Task FakeHttpMessageHandler_Verify_Calls_SynchronousResponseObjectRetriever()
        {
            var responseObjectRetrieverMock = new Mock<IResponceObjectRetriever>();
            responseObjectRetrieverMock.Setup(retriever => retriever.RetrieveResponseObject()).Returns(new FakeOutput());
            var handler = new FakeHttpMessageHandler<FakeOutput>(() => responseObjectRetrieverMock.Object.RetrieveResponseObject());

            using (var httpClient = new HttpClient(handler, true))
            {
                var responce = await httpClient.GetAsync(_requestUri);

                var responseText = await responce.Content.ReadAsStringAsync();
                var clientOutput = JsonConvert.DeserializeObject<FakeOutput>(responseText);

                clientOutput.Should().NotBeNull();
                clientOutput.FakeProperty.Should().NotBeNullOrWhiteSpace();
                handler.CallsCount.Should().Be(1);
                responseObjectRetrieverMock.Verify(retriever => retriever.RetrieveResponseObject(), Times.Once);
            }

        }


        [TestMethod]
        public async Task FakeHttpMessageHandler_Verify_Calls_AsynchronousResponseObjectRetriever()
        {
            var responseObjectRetrieverMock = new Mock<IResponceObjectRetriever>();
            responseObjectRetrieverMock.Setup(retriever => retriever.RetrieveResponseObjectAsync()).ReturnsAsync(new FakeOutput());
            var handler = new FakeHttpMessageHandler<FakeOutput>(() => responseObjectRetrieverMock.Object.RetrieveResponseObjectAsync());

            using (var httpClient = new HttpClient(handler, true))
            {
                var responce = await httpClient.GetAsync(_requestUri);

                var responseText = await responce.Content.ReadAsStringAsync();
                var clientOutput = JsonConvert.DeserializeObject<FakeOutput>(responseText);

                clientOutput.Should().NotBeNull();
                clientOutput.FakeProperty.Should().NotBeNullOrWhiteSpace();
                handler.CallsCount.Should().Be(1);
                responseObjectRetrieverMock.Verify(retriever => retriever.RetrieveResponseObjectAsync(), Times.Once);
            }
        }

        [TestMethod]
        public async Task FakeHttpMessageHandler_Verify_OverridenSerializerIsCalled()
        {
            var responseString = "zheludov.com";
            var serializerMock = new Mock<ISerializer>();
            var responseObjectRetrieverMock = new Mock<IResponceObjectRetriever>();
            serializerMock.Setup(serializer => serializer.Serialize(It.IsAny<object>())).Returns(responseString);

            var handler = new FakeHttpMessageHandler<FakeOutput>(() => responseObjectRetrieverMock.Object.RetrieveResponseObjectAsync(), 
                (FakeOutput output) => serializerMock.Object.Serialize(output));

            using (var httpClient = new HttpClient(handler, true))
            {
                var responce = await httpClient.GetAsync(_requestUri);

                var responseText = await responce.Content.ReadAsStringAsync();
                responseText.Should().NotBeNullOrWhiteSpace();
                responseText.Should().Be(responseString);

                handler.CallsCount.Should().Be(1);
                responseObjectRetrieverMock.Verify(retriever => retriever.RetrieveResponseObjectAsync(), Times.Once);
                serializerMock.Verify(serializer => serializer.Serialize(It.IsAny<object>()),Times.Once);
            }
        }
    }
    
}