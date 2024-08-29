using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Models
{
    public static class Constants
    {
        public const string UrlErrorMessage = "The {0} field is not a valid URL. Ensure you have prefixed the link with http:// or https://";

        public const string DefaultCountry = "AU";

        public const string DefaultTimezone = "Australia/Sydney";

        public static readonly CultureInfo DefaultCulture = CultureInfo.GetCultureInfo("en-AU");

        public const string AllowedGraphicExtensions = "jpg, jpeg, png, gif";

        public const string Email = "hello@example.com"; //TODO this must be completed

        public const string Phone = "02 9999 5555"; //TODO this must be completed or removed
    }

    public static class Descriptions
    {
        public const string Company = "Company";
    }

}
