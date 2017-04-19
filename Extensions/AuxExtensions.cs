using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace CargaOperacao
{
    public static class AuxExtensions
    {
        public static bool ToBool(this string value)
        {
            return (value.Trim().Equals("S") || value.Trim().Equals("1"));
        }

        public static T ObterValor<T>(this XElement pai, string filho)
        {
            var value = pai.Element(filho).Value;

            if (typeof(T) == typeof(bool))
            {
                value = value.ToBool().ToString();
            }
            else if (typeof(T).GetTypeInfo().IsEnum)
            {
                return (T)Enum.Parse(typeof(T), value);
            }
            return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }

        public static string ObterValorOperacao(this XElement pai, string filho)
        {
            return ObterValorOperacao<string>(pai, filho);
        }

        public static T ObterValorOperacao<T>(this XElement pai, string filho)
        {
            return ObterValor<T>(pai.Element("Body").Element("NEGRFPRV0002"), filho);
        }

        public static IEnumerable<XElement> ObterCondicaoResgate(this XElement item)
        {
            return item.Element("Body").Element("NEGRFPRV0002").Element("CondicoesResgate").Elements("CondicaoResgate");
        }

        public static string ObterValorHeader(this XElement pai, string filho)
        {
            return ObterValorHeader<string>(pai, filho);
        }
        public static T ObterValorHeader<T>(this XElement pai, string filho)
        {
            return ObterValor<T>(pai.Element("Header"), filho);
        }

    }
}