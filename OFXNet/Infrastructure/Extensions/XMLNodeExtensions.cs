using System.Xml;

namespace OFXNet.Infrastructure.Extensions
{
    public static class XMLNodeExtensions
    {
        /// <summary>
        /// Returns the value of a specified node in a portion of XML
        /// </summary>
        /// <param name="node">The node that will be searched</param>
        /// <param name="xpath">The path for the node specified</param>
        /// <returns>The value of the specified node, or an empty string if it cannot be found</returns>
        public static string GetValue(this XmlNode node, string xpath)
        {
            XmlNode? tempNode = node.SelectSingleNode(xpath);
            return tempNode?.FirstChild?.Value ?? String.Empty;
        }
    }
}
