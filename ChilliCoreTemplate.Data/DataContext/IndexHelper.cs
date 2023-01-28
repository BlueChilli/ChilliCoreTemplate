using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Data
{
    public static class IndexHelper
    {
        public static decimal IndexForAlphaNumericSorting(string s)
        {
            var result = Decimal.MinValue;
            if (String.IsNullOrEmpty(s)) return result;

            var values = " '0123456789ABCDEFGHIJKLMONPQRSTUVWXYZ";

            var padded = s.ToUpper().PadRight(16).Take(16).Reverse().ToArray();

            for (var i = 0; i < 16; i++)
            {
                var index = Math.Max(values.IndexOf(padded[i]), 0);
                result += index * (decimal)Math.Pow(38, i);
            }
            return result;
        }

        public static Tuple<decimal, decimal> StartsWith(string s)
        {
            return new Tuple<decimal, decimal>(IndexForAlphaNumericSorting(s), IndexForAlphaNumericSorting(s + "Z"));
        }
    }
}
