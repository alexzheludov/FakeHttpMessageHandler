using System.Threading.Tasks;

namespace FakeHttpMessageHandler.Tests.Fakes
{
    public interface IResponceObjectRetriever
    {
        FakeOutput RetrieveResponseObject();
        Task<FakeOutput> RetrieveResponseObjectAsync();
    }
}
