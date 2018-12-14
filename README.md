# FakeHttpMessageHandler

[![Build Status](https://dev.azure.com/ZheludovDevLab/FakeHttpMessageHandler/_apis/build/status/alexzheludov.FakeHttpMessageHandler?branchName=master)](https://dev.azure.com/ZheludovDevLab/FakeHttpMessageHandler/_build/latest?definitionId=2?branchName=master)

FakeHttpMessageHandler seamlessly fakes http request sent from System.Net.Http.HttpClient for purpose of unit testing code that previously could only be integration tested.

## Examples

If FakeHttpMessageHandler is initialized with no parameters, it will create an instance of T, serialize it as JSON and return it in a content of HttpResponse.
```
using(var httpMessageHandler = new FakeHttpMessageHandler<OutputType>())
using(var httpClient = new HttpClient(httpMessageHandler))
{
    var response = await httpClient.GetAsync("http://zheludov.com");
}
```

You may also choose to override returned object by either passing an instance of returned object to a constructor or a function that will retrieve it. In both cases an object will be serialized into JSON unless serializer is overwritten or T is string;


```
// Passing an instance of output object
var output = new OutputType();
using(var httpMessageHandler = new FakeHttpMessageHandler<OutputType>(output))
using(var httpClient = new HttpClient(httpMessageHandler))
{
    var response = await httpClient.GetAsync("http://zheludov.com");
}


// Passing a function that will retrieve output object.
var outputRetrieverMock = new Mock<IResponceObjectRetriever>();
outputRetrieverMock.Setup(retriever => retriever.RetrieveResponseObject()).Returns(new FakeOutput());

using (var httpMessageHandler = new FakeHttpMessageHandler<FakeOutput>(() => outputRetrieverMock.Object.RetrieveResponseObject()))
using (var httpClient = new HttpClient(httpMessageHandler))
{
    var response = await httpClient.GetAsync("http://zheludov.com");
}


// Passing an asynchronous function that will retrieve output object.
var outputRetrieverMock = new Mock<IResponceObjectRetriever>();
outputRetrieverMock.Setup(retriever => retriever.RetrieveResponseObjectAsync()).ReturnsAsync(new FakeOutput());

using (var httpMessageHandler = new FakeHttpMessageHandler<FakeOutput>(() => outputRetrieverMock.Object.RetrieveResponseObject()))
using (var httpClient = new HttpClient(httpMessageHandler))
{
    var response = await httpClient.GetAsync("http://zheludov.com");
}
```

Additionally you can override serializer function that is called to serilize response content.
```
var responseString = "zheludov.com";
var serializerMock = new Mock<ISerializer>();
var responseObjectRetrieverMock = new Mock<IResponceObjectRetriever>();
serializerMock.Setup(serializer => serializer.Serialize(It.IsAny<object>())).Returns(responseString);

var handler = new FakeHttpMessageHandler<FakeOutput>(() => responseObjectRetrieverMock.Object.RetrieveResponseObjectAsync(), 
    (FakeOutput output) => serializerMock.Object.Serialize(output));
using (var httpClient = new HttpClient(httpMessageHandler))
{
    var response = await httpClient.GetAsync("http://zheludov.com");
}
```


### Sample Class and Unit Test

#### ApiClient.cs
```
public class ApiClient : IDisposable
{
    private readonly HttpMessageHandler _httpMessageHandler;
    public ApiClient(HttpMessageHandler httpMessageHandler = null)
    {
        _httpMessageHandler = httpMessageHandler;
    }

    public async Task<string> GetResponseAsync()
    {
        using (var httpClient = new HttpClient(_httpMessageHandler ?? new HttpClientHandler()))
        {
            var response = await httpClient.GetAsync("http://zheludov.com");
            if(response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return null;
        }
    }

    public void Dispose()
    {
        _httpMessageHandler?.Dispose();
    }
}
```

#### ApiClientTest.cs
```
 [TestClass]
public class ApiClientTest
{
    [TestMethod]
    public async Task ApiClient_GetResponseAsync_VerifyHttpRequestIsMade()
    {
        var httpHandler = new FakeHttpMessageHandler<string>("Response");
        using (var client = new ApiClient(httpHandler))
        {
            var response = await client.GetResponseAsync();
            response.Should().NotBeNullOrWhiteSpace();
        }    
    }
}
```