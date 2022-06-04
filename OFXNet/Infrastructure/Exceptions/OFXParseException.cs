using System.Runtime.Serialization;

namespace OFXNet.Infrastructure.Exceptions
{
    public class OFXParseException : Exception
    {
        public OFXParseException(string message) : base(message) { }

        public OFXParseException(string message, Exception innerException) : base(message, innerException) { }
    }
}