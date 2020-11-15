using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ServiceStack.Host;

namespace EthernetSwitch.Infrastructure.Extensions
{
    public static class StringExtensions
    {
        public static bool IsEmpty(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static bool IsNotEmpty(this string value)
        {
            return !value.IsEmpty();
        }

        public static string FormatWith(this string format, object source)
        {
            return FormatWith(format, null, source);
        }

        public static string FormatWith(this string format, IFormatProvider provider, object source)
        {
            if (format == null)
                throw new ArgumentNullException("format");

            var regex = new Regex(
                @"(?<start>\{)+(?<property>[\w\.\[\]]+)(\((?<truevalue>\w+)\|(?<falsevalue>\w+)\))?(?<format>:[^}]+)?(?<end>\})+",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            var values = new List<object>();
            var rewrittenFormat = regex.Replace(format, match =>
            {
                Group startGroup = match.Groups["start"];
                Group propertyGroup = match.Groups["property"];
                Group truevalue = match.Groups["truevalue"];
                Group falsevalue = match.Groups["falsevalue"];
                Group formatGroup = match.Groups["format"];
                Group endGroup = match.Groups["end"];


                if (propertyGroup.Value == "0")
                {
                    values.Add(source);
                }
                else if (truevalue.Success && falsevalue.Success)
                {
                    values.Add((bool)DataBinder.Eval(source, propertyGroup.Value)
                        ? truevalue.Value
                        : falsevalue.Value);

                }
                else values.Add(DataBinder.Eval(source, propertyGroup.Value));

                return new string('{', startGroup.Captures.Count) + (values.Count - 1) + formatGroup.Value + new string('}', endGroup.Captures.Count);
            });

            return string.Format(provider, rewrittenFormat, values.ToArray());
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}
