using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FakeHttpMessageHandler
{
    /// <summary>
    /// Fake implementation of abstract HttpMessageHandler
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FakeHttpMessageHandler<T> : HttpMessageHandler where T : class
    {
        private readonly T _overrideResponseContent;

        public FakeHttpMessageHandler(T overrideResponseContent = null)
        {
            _overrideResponseContent = overrideResponseContent;
        }

        /// <summary>
        /// Count of calls made
        /// </summary>
        public int CallsCount { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallsCount++;

            T returnObject = null;
            returnObject = _overrideResponseContent ?? Activator.CreateInstance<T>();
            var returnObjectString = returnObject is string ? returnObject as string : JsonConvert.SerializeObject(returnObject);
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(Encoding.ASCII.GetBytes(returnObjectString))
            };

            return Task.FromResult(responseMessage);
        }

        /// <summary>
        /// Resets calls count
        /// </summary>
        public void ResetCalls()
        {
            CallsCount = 0;
        }
    }
}
