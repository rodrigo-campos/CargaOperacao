using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace CargaOperacao
{
    public static class AuxExtensions
    {
        public static bool ToBool(this string value)
        {
            return (value.Trim().Equals("S") || value.Trim().Equals("1"));
        }

        public static DateTime ToDateTime(this string value)
        {
            return DateTime.Parse(value);
        }

        public static decimal ToDecimal(this string value)
        {
            return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }

        public static string GetValue(this XElement item, string element)
        {
            return item.Descendants().Elements().Any() ? item.Descendants().Elements().First(i => i.Name == element).Value : item.Descendants().First(i => i.Name == element).Value;
        }

    }
}