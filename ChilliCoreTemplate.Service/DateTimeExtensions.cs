using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service
{
    public static class DateTimeExtensions
    {
            /// <summary>
        /// Converts the value of specified date from user time zone to server time zone. (Used to Convert user entered date to server time zone, so data is stored in server time).
        /// </summary>
        /// <param name="value">The specified date.</param>
        /// <param name="userTimeZone">The specified user time zone.</param>
        /// <param name="serverTimezone">The specified server time zone.</param>
        /// <returns>A value of specified date in specified server time zone.</returns>
        public static DateTime FromUserTimezone(this DateTime value, string userTimeZone = "Australia/Sydney", string serverTimezone = "UTC")
        {
            //TODO get values from web.config
            return value.ToTimezone(serverTimezone, userTimeZone);
        }
    }
}
