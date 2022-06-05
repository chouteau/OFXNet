using OFXNet.Utils;

namespace OFXNet.Tests.Utils
{
    [TestClass]
    public class OFXDocumentParserTests
    {
        [TestMethod]
        public void OFXDocumentParser_CanParse_SGMLOFX()
        {
            _ = OFXDocumentParser.Import(new FileStream(@"TestFiles/SGMLOFX.ofx", FileMode.Open));
        }

        [TestMethod]
        public void OFXDocumentParser_CanParse_XMLOFX()
        {
            _ = OFXDocumentParser.Import(new FileStream(@"TestFiles/XMLOFX.ofx", FileMode.Open));
        }
    }
}