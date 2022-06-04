using OFXNet.Enums;
using OFXNet.Infrastructure.Exceptions;
using OFXNet.Infrastructure.Extensions;
using OFXNet.Properties;
using System.Text;
using System.Xml;

namespace OFXNet.Models
{
    public class OFXDocumentParser
    {
        public OFXDocument Import(FileStream stream)
        {
            using StreamReader reader = new(stream, Encoding.Default);
            return Import(reader.ReadToEnd());
        }

        public OFXDocument Import(string ofx)
        {
            return ParseOfxDocument(ofx);
        }

        private OFXDocument ParseOfxDocument(string ofxString)
        {
            //If OFX file in SGML format, convert to XML
            if (!IsXmlVersion(ofxString))
            {
                ofxString = SGMLToXML(ofxString);
            }

            return Parse(ofxString);
        }

        private OFXDocument Parse(string ofxString)
        {
            OFXDocument ofx = new() { AccType = GetAccountType(ofxString) };

            //Load into xml document
            XmlDocument doc = new();
            doc.Load(new StringReader(ofxString));

            XmlNode? currencyNode = doc.SelectSingleNode(GetXPath(ofx.AccType, OFXSection.CURRENCY));

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
            var accountNode = doc.SelectSingleNode(GetXPath(ofx.AccType, OFXSection.ACCOUNTINFO));

            //If account info present, populate account object
            if (accountNode != null)
            {
                ofx.Account = new Account(accountNode, ofx.AccType);
            }
            else
            {
                throw new OFXParseException("Account information not found");
            }

            //Get list of transactions
            ImportTransations(ofx, doc);

            //Get balance info from ofx doc
            var ledgerNode = doc.SelectSingleNode(GetXPath(ofx.AccType, OFXSection.BALANCE) + "/LEDGERBAL");
            var avaliableNode = doc.SelectSingleNode(GetXPath(ofx.AccType, OFXSection.BALANCE) + "/AVAILBAL");

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
            var xpath = GetXPath(ofxDocument.AccType, OFXSection.TRANSACTIONS);

            ofxDocument.StatementStart = doc.GetValue(xpath + "//DTSTART").ToDate();
            ofxDocument.StatementEnd = doc.GetValue(xpath + "//DTEND").ToDate();

            var transactionNodes = doc.SelectNodes(xpath + "//STMTTRN");

            ofxDocument.Transactions = new List<Transaction>();

            foreach (XmlNode node in transactionNodes)
                ofxDocument.Transactions.Add(new Transaction(node, ofxDocument.Currency));
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
            if (header[0] != "OFXHEADER:100")
                throw new OFXParseException("Incorrect header format");

            if (header[1] != "DATA:OFXSGML")
                throw new OFXParseException("Data type unsupported: " + header[1] + ". OFXSGML required");

            if (header[2] != "VERSION:102")
                throw new OFXParseException("OFX version unsupported. " + header[2]);

            if (header[3] != "SECURITY:NONE")
                throw new OFXParseException("OFX security unsupported");

            if (header[4] != "ENCODING:USASCII")
                throw new OFXParseException("ASCII Format unsupported:" + header[4]);

            if (header[5] != "CHARSET:1252")
                throw new OFXParseException("Charecter set unsupported:" + header[5]);

            if (header[6] != "COMPRESSION:NONE")
                throw new OFXParseException("Compression unsupported");

            if (header[7] != "OLDFILEUID:NONE")
                throw new OFXParseException("OLDFILEUID incorrect");
        }
    }
}
