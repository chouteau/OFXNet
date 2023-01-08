using OFXNet.Enums;
using OFXNet.Infrastructure.Exceptions;
using OFXNet.Infrastructure.Extensions;
using OFXNet.Models;
using OFXNet.Properties;
using System.Text;
using System.Xml;

namespace OFXNet.Utils
{
    public class OFXDocumentParser
    {
        /// <summary>
        /// Imports an OFX file and returns a <see cref="OFXDocument">OFXDocument</see>.
        /// </summary>
        /// <param name="stream">The stream pointing to OFX file.</param>
        /// <returns>The parsed OFXDocument</returns>
        public static OFXDocument Import(FileStream stream)
        {
            using StreamReader reader = new(stream, Encoding.Default);
            return Import(reader.ReadToEnd());
        }

        /// <summary>
        /// Internal method used by <see cref="Import">Import</see>. Can be used directly in the file is already
        /// available as a string.
        /// </summary>
        /// <param name="ofx">The OFX file as a string.</param>
        /// <returns>The parsed OFXDocument</returns>
        public static OFXDocument Import(string ofx)
        {
            return ParseOfxDocument(ofx);
        }

        /// <summary>
        /// Determines if the document needs to be converted to XML before parsing, before calling the <see cref="Parse">Parse</see> method to
        /// parse the document.
        /// </summary>
        /// <param name="ofx">The OFX file as a string.</param>
        /// <returns>The parsed OFXDocument</returns>
        private static OFXDocument ParseOfxDocument(string ofxString)
        {
            //If OFX file in SGML format, convert to XML
            if (!IsXmlVersion(ofxString))
            {
                ofxString = SGMLToXML(ofxString);
            }

            return Parse(ofxString);
        }

        private static OFXDocument Parse(string ofxString)
        {
            OFXDocument ofx = new() { AccountType = GetAccountType(ofxString) };

            //Load into xml document
            XmlDocument doc = new();
            doc.Load(new StringReader(ofxString));

            XmlNode? currencyNode = doc.SelectSingleNode(GetXPath(ofx.AccountType, OFXSection.CURRENCY));

            if (currencyNode != null && currencyNode.FirstChild != null)
            {
                ofx.Currency = currencyNode.FirstChild.Value;
            }
            else
            {
                throw new OFXParseException("Currency not found");
            }

            //Get sign on node from OFX file
            var signOnNode = doc.SelectSingleNode(Resources.SignOn);

            //If exists, populate signon obj, else throw parse error
            if (signOnNode != null)
            {
                ofx.SignOn = new SignOn(signOnNode);
            }
            else
            {
                throw new OFXParseException("Sign On information not found");
            }

            //Get Account information for ofx doc
            var accountNode = doc.SelectSingleNode(GetXPath(ofx.AccountType, OFXSection.ACCOUNTINFO));

            //If account info present, populate account object
            if (accountNode != null)
            {
                ofx.Account = new Account(accountNode, ofx.AccountType);
            }
            else
            {
                throw new OFXParseException("Account information not found");
            }

            //Get list of transactions
            ImportTransations(ofx, doc);

            //Get balance info from ofx doc
            var ledgerNode = doc.SelectSingleNode(GetXPath(ofx.AccountType, OFXSection.BALANCE) + "/LEDGERBAL");
            var avaliableNode = doc.SelectSingleNode(GetXPath(ofx.AccountType, OFXSection.BALANCE) + "/AVAILBAL");

            //If balance info present, populate balance object
            if (ledgerNode != null)
            {
                ofx.Balance = new Balance(ledgerNode, avaliableNode);
            }
            else
            {
                throw new OFXParseException("Balance information not found");
            }

            return ofx;
        }


        /// <summary>
        /// Returns the correct xpath to specified section for given account type
        /// </summary>
        /// <param name="type">Account type</param>
        /// <param name="section">Section of OFX document, e.g. Transaction Section</param>
        /// <exception cref="OFXException">Thrown in account type not supported</exception>
        private static string GetXPath(AccountType type, OFXSection section)
        {
            string xpath, accountInfo;

            switch (type)
            {
                case AccountType.BANK:
                    xpath = Resources.BankAccount;
                    accountInfo = "/BANKACCTFROM";
                    break;
                case AccountType.CC:
                    xpath = Resources.CCAccount;
                    accountInfo = "/CCACCTFROM";
                    break;
                default:
                    throw new OFXException("Account Type not supported. Account type " + type);
            }

            return section switch
            {
                OFXSection.ACCOUNTINFO => xpath + accountInfo,
                OFXSection.BALANCE => xpath,
                OFXSection.TRANSACTIONS => xpath + "/BANKTRANLIST",
                OFXSection.SIGNON => Resources.SignOn,
                OFXSection.CURRENCY => xpath + "/CURDEF",
                _ => throw new OFXException("Unknown section found when retrieving XPath. Section " + section),
            };
        }

        /// <summary>
        /// Returns list of all transactions in OFX document
        /// </summary>
        /// <param name="doc">OFX document</param>
        /// <returns>List of transactions found in OFX document</returns>
        private static void ImportTransations(OFXDocument ofxDocument, XmlDocument doc)
        {
            var xpath = GetXPath(ofxDocument.AccountType, OFXSection.TRANSACTIONS);

            ofxDocument.StatementStart = doc.GetValue(xpath + "//DTSTART").ToDate();
            ofxDocument.StatementEnd = doc.GetValue(xpath + "//DTEND").ToDate();

            XmlNodeList? transactionNodes = doc.SelectNodes(xpath + "//STMTTRN");

            if (transactionNodes != null)
            {
                foreach (XmlNode node in transactionNodes)
                    ofxDocument.Transactions.Add(new Transaction(node, ofxDocument.Currency ?? null));
            }
        }


        /// <summary>
        /// Checks account type of supplied file
        /// </summaryof
        /// <param name="file">OFX file want to check</param>
        /// <returns>Account type for account supplied in ofx file</returns>
        private static AccountType GetAccountType(string file)
        {
            if (file.IndexOf("<CREDITCARDMSGSRSV1>") != -1)
                return AccountType.CC;

            if (file.IndexOf("<BANKMSGSRSV1>") != -1)
                return AccountType.BANK;

            throw new OFXException("Unsupported Account Type");
        }

        /// <summary>
        /// Check if OFX file is in SGML or XML format
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static bool IsXmlVersion(string file)
        {
            return !file.Contains("OFXHEADER:100", StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Converts SGML to XML
        /// </summary>
        /// <param name="file">OFX File (SGML Format)</param>
        /// <returns>OFX File in XML format</returns>
        private static string SGMLToXML(string file)
        {
            SgmlReader.Logic.SgmlReader reader = new()
            {
                //Inititialize SGML reader
                InputStream = new StringReader(ParseHeader(file)),
                DocType = "OFX"
            };

            var sw = new StringWriter();
            var xml = new XmlTextWriter(sw);

            //write output of sgml reader to xml text writer
            while (!reader.EOF)
                xml.WriteNode(reader, true);

            //close xml text writer
            xml.Flush();
            xml.Close();

            var temp = sw.ToString().TrimStart().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            return string.Join("", temp);
        }

        /// <summary>
        /// Checks that the file is supported by checking the header. Removes the header.
        /// </summary>
        /// <param name="file">OFX file</param>
        /// <returns>File, without the header</returns>
        private static string ParseHeader(string file)
        {
            //Select header of file and split into array
            //End of header worked out by finding first instance of '<'
            //Array split based of new line & carrige return
            var header = file[..file.IndexOf('<')]
               .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            //Check that no errors in header
            CheckHeader(header);

            //Remove header
            return file[file.IndexOf('<')..].Trim();
        }

        /// <summary>
        /// Checks that all the elements in the header are supported
        /// </summary>
        /// <param name="header">Header of OFX file in array</param>
        private static void CheckHeader(string[] header)
        {
            if (header[0] == "OFXHEADER:100DATA:OFXSGMLVERSION:102SECURITY:NONEENCODING:USASCIICHARSET:1252COMPRESSION:NONEOLDFILEUID:NONENEWFILEUID:NONE")//non delimited header
                return;

            var headers = from item in header
                          let part = item.Split(':')
                          select new
                          {
                              Name = part[0].Trim().ToUpper(),
                              Value = part[1].Trim().ToUpper(),
                              Header = item
                          };

            if (!headers.Any(i => i.Name == "OFXHEADER" && i.Value == "100"))
            {
                throw new OFXParseException("Incorrect header format");
            }

            if (!headers.Any(i => i.Name == "DATA" && i.Value == "OFXSGML"))
            {
                throw new OFXParseException("Data type unsupported: DATA. OFXSGML required");
            }

            if (!headers.Any(i => i.Name == "VERSION" && i.Value == "102"))
            {
                throw new OFXParseException("OFX version unsupported. " + headers.Single(i => i.Name == "VERSION").Header);
            }

            if (!headers.Any(i => i.Name == "SECURITY" && i.Value == "NONE"))
            {
                throw new OFXParseException("OFX security unsupported");
            }

            if (!headers.Any(i => i.Name == "ENCODING" && i.Value == "USASCII"))
            {
                throw new OFXParseException("ASCII Format unsupported:" + headers.Single(i => i.Name == "ENCODING").Header);
            }

            if (!headers.Any(i => i.Name == "CHARSET" && i.Value == "1252"))
            {
                throw new OFXParseException("Charecter set unsupported:" + headers.Single(i => i.Name == "CHARSET").Header);
            }

            if (!headers.Any(i => i.Name == "COMPRESSION" && i.Value == "NONE"))
            {
                throw new OFXParseException("Compression unsupported");
            }

            if (!headers.Any(i => i.Name == "OLDFILEUID" && i.Value == "NONE"))
            {
                throw new OFXParseException("OLDFILEUID incorrect");
            }
        }
    }
}
