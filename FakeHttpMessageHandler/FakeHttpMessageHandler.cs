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
        private readonly Func<T> _createResponseObjectFunction;
        private readonly Func<Task<T>> _createResponseObjectAsyncFunction;
        private readonly Func<T, string> _serializerFunction;
        private readonly HttpStatusCode _httpStatusCode;

        /// <summary>
        /// FakeHttpMessageHandler seamlessly fakes http request sent from System.Net.Http.HttpClient for purpose of unit testing code that previously could only be integration tested.
        /// Passing <paramref name="overrideResponseContent"/> will override response of a message handler to JSON serialized representation of <paramref name="overrideResponseContent"/>
        /// </summary>
        /// <param name="overrideResponseContent"></param>
        /// <param name="serializerFunction"></param>
        /// <param name="httpStatusCode"></param>
        public FakeHttpMessageHandler(T overrideResponseContent = null, Func<T, string> serializerFunction = null, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
        {
            _overrideResponseContent = overrideResponseContent;
            _serializerFunction = serializerFunction ?? new Func<T, string>((T output) => JsonConvert.SerializeObject(output));
            _httpStatusCode = httpStatusCode;
        }

        /// <summary>
        /// FakeHttpMessageHandler seamlessly fakes http request sent from System.Net.Http.HttpClient for purpose of unit testing code that previously could only be integration tested.
        /// <paramref name="createResponseObjectFunction"/> will be called to generate response object, and response message handler will return a JSON representation of the object that was created by <paramref name="createResponseObjectFunction"/>
        /// </summary>
        /// <param name="createResponseObjectFunction"></param>
        /// <param name="serializerFunction"></param>
        /// <param name="httpStatusCode"></param>
        public FakeHttpMessageHandler(Func<T> createResponseObjectFunction, Func<T, string> serializerFunction = null, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
        {
            _createResponseObjectFunction = createResponseObjectFunction ?? throw new ArgumentNullException(nameof(createResponseObjectFunction));
            _serializerFunction = serializerFunction ?? new Func<T, string>((T output) => JsonConvert.SerializeObject(output));
            _httpStatusCode = httpStatusCode;
        }

        /// <summary>
        /// FakeHttpMessageHandler seamlessly fakes http request sent from System.Net.Http.HttpClient for purpose of unit testing code that previously could only be integration tested.
        /// <paramref name="createResponseObjectAsyncFunction"/> will be called to generate response object, and response message handler will return a JSON representation of the object that was created by <paramref name="createResponseObjectAsyncFunction"/>
        /// </summary>
        /// <param name="createResponseObjectAsyncFunction"></param>
        /// <param name="serializerFunction"></param>
        /// <param name="httpStatusCode"></param>
        public FakeHttpMessageHandler(Func<Task<T>> createResponseObjectAsyncFunction, Func<T, string> serializerFunction = null, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
        {
            _createResponseObjectAsyncFunction = createResponseObjectAsyncFunction ?? throw new ArgumentNullException(nameof(createResponseObjectAsyncFunction));
            _serializerFunction = serializerFunction ?? new Func<T, string>((T output) => JsonConvert.SerializeObject(output));
            _httpStatusCode = httpStatusCode;
        }

        /// <summary>
        /// Count of calls made
        /// </summary>
        public int CallsCount { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallsCount++;

            T returnObject = null;
            if (_createResponseObjectFunction != null)
            {
                returnObject = _createResponseObjectFunction();
            }
            else if(_createResponseObjectAsyncFunction != null)
            {
                returnObject = await _createResponseObjectAsyncFunction();
            }
            else
            {
                returnObject = _overrideResponseContent ?? Activator.CreateInstance<T>();
            }
            var returnObjectString = returnObject is string ? returnObject as string : _serializerFunction(returnObject);
            HttpResponseMessage responseMessage = new HttpResponseMessage(_httpStatusCode)
            {
                Content = new ByteArrayContent(Encoding.ASCII.GetBytes(returnObjectString))
            };

            return responseMessage;
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
