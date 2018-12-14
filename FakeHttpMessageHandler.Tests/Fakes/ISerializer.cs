using System;
using System.Collections.Generic;
using System.Text;

namespace FakeHttpMessageHandler.Tests.Fakes
{
    public interface ISerializer
    {
        string Serialize(object objectToSerialize);
    }
}
