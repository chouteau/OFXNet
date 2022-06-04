using System.ComponentModel;

namespace OFXNet.Enums
{
    public enum AccountType
    {
        [Description("Bank Account")]
        BANK,
        [Description("Credit Card")]
        CC,
        [Description("Accounts Payable")]
        AP,
        [Description("Accounts Recievable")]
        AR,
        [Description("Not Applicable")]
        NA,
    }
}
