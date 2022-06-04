namespace OFXNet.Tests
{
    [TestClass]
    public class OFXDocumentParserTests
    {
        [TestMethod]
        public void OFXDocumentParser_CanParse_SGMLOFX()
        {
            var parser = new OFXDocumentParser();
            _ = parser.Import(new FileStream(@"TestFiles/SGMLOFX.ofx", FileMode.Open));
        }

        [TestMethod]
        public void OFXDocumentParser_CanParse_XMLOFX()
        {
            var parser = new OFXDocumentParser();
            _ = parser.Import(new FileStream(@"TestFiles/XMLOFX.ofx", FileMode.Open));
        }
    }
}