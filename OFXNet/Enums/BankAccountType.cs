using System.ComponentModel;

namespace OFXNet.Enums
{
    public enum BankAccountType
    {
        [Description("Checking Account")]
        CHECKING,
        [Description("Savings Account")]
        SAVINGS,
        [Description("Money Market Account")]
        MONEYMRKT,
        [Description("Line of Credit")]
        CREDITLINE,
        [Description("Not Applicable")]
        NA,
        [Description("Home Loan")]
        HOMELOAN,
    }
}
