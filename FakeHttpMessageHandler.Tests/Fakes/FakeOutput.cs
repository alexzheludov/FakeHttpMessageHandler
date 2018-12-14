namespace FakeHttpMessageHandler.Tests.Fakes
{
    public class FakeOutput
    {
        public FakeOutput()
        {
            FakeProperty = "FakePropertyValue";
        }
        public string FakeProperty { get; set; }
    }
}
