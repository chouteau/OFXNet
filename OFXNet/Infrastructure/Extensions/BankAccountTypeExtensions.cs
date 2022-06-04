using OFXNet.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFXNet.Infrastructure.Extensions
{
    public static class BankAccountTypeExtensions
    {
        /// <summary>
        /// Converts string representation of AccountInfo to enum AccountInfo
        /// </summary>
        /// <param name="bankAccountType">representation of AccountInfo</param>
        /// <returns>AccountInfo</returns>
        public static BankAccountType GetBankAccountType(this string bankAccountType)
        {
            try
            {
                return (BankAccountType)Enum.Parse(typeof(BankAccountType), bankAccountType, true);
            }
            catch (Exception)
            {
                return BankAccountType.NA;
            }
        }
    }
}
