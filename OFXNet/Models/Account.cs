using OFXNet.Enums;
using OFXNet.Infrastructure.Exceptions;
using OFXNet.Infrastructure.Extensions;
using System.Xml;

namespace OFXNet.Models
{
    public class Account
    {
        public string AccountID { get; set; }
        public string AccountKey { get; set; }
        public AccountType AccountType { get; set; }
        public string? BankID { get; set; }
        public string? BranchID { get; set; }
        public BankAccountType BankAccountType { get; set; }

        public Account(XmlNode node, AccountType type)
        {
            AccountType = type;
            AccountID = node.GetValue("//ACCTID");
            AccountKey = node.GetValue("//ACCTKEY");

            switch (AccountType)
            {
                case AccountType.BANK:
                    InitializeBank(node);
                    break;
                case AccountType.AP:
                    throw new OFXParseException("Account type not supported.");
                case AccountType.AR:
                    throw new OFXParseException("Account type not supported.");
                default:
                    // Not BANK, AP or AR
                    BankAccountType = BankAccountType.NA;
                    break;
            }
        }

        /// <summary>
        /// Initialise the bank details for this account. Bank details are optional and only set if included in the OFX File.
        /// </summary>
        /// <param name="node">The bank node</param>
        /// <exception cref="OFXParseException">If the bank account type cannot be read</exception>
        private void InitializeBank(XmlNode node)
        {
            BankID = node.GetValue("//BANKID");
            BranchID = node.GetValue("//BRANCHID");

            //Get Bank Account Type from XML
            string bankAccountType = node.GetValue("//ACCTTYPE");

            //Check that it has been set
            if (string.IsNullOrEmpty(bankAccountType))
                throw new OFXParseException("Bank Account type unknown");

            //Set bank account enum
            bool parseSuccessful = Enum.TryParse<BankAccountType>(bankAccountType, true, out BankAccountType value);
            if (parseSuccessful)
            {
                BankAccountType = value;
            }
            else
            {
                BankAccountType = BankAccountType.NA;
            }
        }
    }
}