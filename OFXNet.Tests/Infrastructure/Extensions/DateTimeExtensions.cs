using OFXNet.Infrastructure.Exceptions;
using OFXNet.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFXNet.Tests.Infrastructure.Extensions
{
    [TestClass]
    public class DateTimeExtensions
    {
        [TestMethod]
        public void ToDate_ReturnsValidDate_OnValidInput()
        {
            // Arrange
            string input = "20200102";

            // Act
            DateTime output = input.ToDate();

            // Assert
            Assert.AreEqual(2020, output.Year);
            Assert.AreEqual(1, output.Month);
            Assert.AreEqual(2, output.Day);
        }

        [TestMethod]
        public void ToDate_ThrowsOFXException_OnInvalidInput()
        {
            // Arrange
            string input = "TESTTEST";

            // Act & Assert
            Assert.ThrowsException<OFXParseException>(() => input.ToDate());
        }
    }
}
