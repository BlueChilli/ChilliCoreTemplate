using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.Phone;
using ChilliSource.Cloud.Web.MVC;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using NodaTime;
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

        public static DateTimeZone DefaultDateTimezone => DateTimeZoneProviders.Tzdb[DefaultTimezone];


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

        public static IReadOnlyDictionary<string, object> AddRouteValues(this IReadOnlyDictionary<string, object> route, IDictionary<string, string> routeValues)
        {
            if (routeValues == null || routeValues.Count == 0) return route;

            var result = new RouteValueDictionary(route);
            foreach (var set in routeValues) result[set.Key] = set.Value;

            return result;
        }

        public static SelectList ToSelectList(this List<DataLinkModel> list)
        {
            return list.ToSelectList(v => v.Id, t => t.Name);
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (action == null) throw new ArgumentNullException("action");
            foreach (T element in source)
            {
                action(element);
            }
        }
    }
}
