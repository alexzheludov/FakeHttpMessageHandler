using FakeHttpMessageHandler;
using FakeHttpMessageHandler.Tests.Fakes;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
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
    }
}
