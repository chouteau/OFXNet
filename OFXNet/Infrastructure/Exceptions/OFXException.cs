using System.Runtime.Serialization;

namespace OFXNet.Infrastructure.Exceptions
{
    public class OFXException : Exception
    {
        public OFXException(string message) : base(message) { }
    }
}