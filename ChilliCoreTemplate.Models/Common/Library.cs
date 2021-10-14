using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.Phone;
using ChilliSource.Core.Extensions;
using NodaTime.TimeZones;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Models
{
    public static class CommonLibrary
    {
        public static int? CalculateHash(string source) { return source == null ? null : source.ToLowerInvariant().GetIndependentHashCode(); }


        public const string DefaultTimezone = "Australia/Sydney";

        //TODO use if possible current user timezone
        public static DateTime ToTimezone(this DateTime dt)
        {
            return dt.ToTimezone(DefaultTimezone);
        }

        public static DateTime? ToTimezone(this DateTime? dt)
        {
            return dt.HasValue ? (DateTime?)dt.Value.ToTimezone(DefaultTimezone) : null;
        }

        public static DateTime ToUtcTimezone(this DateTime dt)
        {
            return dt.ToUtcTimezone(DefaultTimezone);
        }

        public static DateTime? ToUtcTimezone(this DateTime? dt)
        {
            return dt.HasValue ? (DateTime?)dt.Value.ToUtcTimezone(DefaultTimezone) : null;
        }

        public static string ToFormattedDateTime(this DateTime dt)
        {
            return dt.ToString("dd MMM yyyy HH:mm");
        }

        public static List<TzdbZoneLocation> TimeZones()
        {
            return TzdbDateTimeZoneSource.Default
                .ZoneLocations
                .OrderBy(x => x.CountryName)
                .ToList();
        }

        public static DateTime PreviousDayOfWeek(this DateTime date, DayOfWeek previousWeekDay = DayOfWeek.Sunday)
        {
            int diff = date.DayOfWeek - previousWeekDay;
            if (diff < 0)
            {
                diff += 7;
            }
            return date.AddDays(-diff).Date;
        }

        public static string PhoneFormat(string phone, string region)
        {
            if (String.IsNullOrEmpty(phone)) return phone;
            if (region.Length > 2)
            {
                region = new RegionInfo(region).TwoLetterISORegionName;
            }

            var util = PhoneNumberUtil.GetInstance();
            try
            {
                var phoneNumber = util.Parse(phone, region);
                return $"+{phoneNumber.FormatPhoneNumber(PhoneNumberFormat.INTERNATIONAL)}";
            }
            catch
            {
                return phone;
            }
        }

    }
}
