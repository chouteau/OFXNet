using OFXNet.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace OFXNet.Tests.Infrastructure.Extensions
{
    [TestClass]
    public class XMLNodeExtensionsTests
    {
        [TestMethod]
        public void GetValue_ReturnsExpectedValue_OnValidInput()
        {
            // Arrange
            XmlDocument xml = new();
            xml.LoadXml(File.ReadAllText(@"TestFiles/TestXml.xml"));
            XmlNode? node = xml.ChildNodes[1];

            Assert.IsNotNull(node);

            string xPath = "ChildElement//SubChildElement//Value";

            // Act
            string outputValue = node.GetValue(xPath);

            // Assert
            Assert.AreEqual("TestValue", outputValue);
        }

        [TestMethod]
        public void GetValue_ReturnsEmptyString_OnInvalidInput()
        {
            // Arrange
            XmlDocument xml = new();
            xml.LoadXml(File.ReadAllText(@"TestFiles/TestXml.xml"));
            XmlNode? node = xml.ChildNodes[1];

            Assert.IsNotNull(node);

            string xPath = "ChildElement//SubChildElement//InvalidValue";

            // Act
            string outputValue = node.GetValue(xPath);

            // Assert
            Assert.AreEqual(String.Empty, outputValue);
        }
    }
}
