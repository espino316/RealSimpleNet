using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RealSimpleNet.Helpers
{
    public class Date
    {
        /// <summary>
        /// Devuleve un entero, representando un valor UnixTime
        /// </summary>
        /// <returns></returns>
        public static int GetUnixTime()
        {
            TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            double unixTime = ts.TotalSeconds;
            return Convert.ToInt32(unixTime);
        } // end static unix time

        public static DateTime StartDay(DateTime date)
        {
            return date.Date;
        }

        public static DateTime EndDay(DateTime date)
        {
            return date.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
        }

    } // end class DateHelper
}
