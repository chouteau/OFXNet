using OFXNet.Infrastructure.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFXNet.Infrastructure.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// OFX Dates are in format of 'yyymmdd'. This method converts this format into a valid date
        /// </summary>
        /// <param name="date">The input date as a string in format YYYMMDD</param>
        /// <returns></returns>
        /// <exception cref="OFXParseException">Throws when an error occured parsing the date</exception>
        public static DateTime ToDate(this string date)
        {
            try
            {
                if (date.Length < 8)
                {
                    return new DateTime();
                }

                int day = Int32.Parse(date.Substring(6, 2));
                int month = Int32.Parse(date.Substring(4, 2));
                int year = Int32.Parse(date[..4]);

                return new DateTime(year, month, day);
            }
            catch
            {
                throw new OFXParseException("Unable to parse date");
            }
        }
    }
}
