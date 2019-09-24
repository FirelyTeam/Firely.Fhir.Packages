using System;
using System.Collections.Generic;

namespace Hl7.Fhir.Packages
{
    public static class PackageUtils
    {

    }

    public static class Helpers
    {
        // Missing in netstandard.
        public static string[] Split(this string s, string separator)
        {
            return s.Split(new string[] { separator }, StringSplitOptions.None);
        }

        public static string Coalesce(this string s, params string[] values)
        {
            foreach(string value in values)
            {
                if (!(string.IsNullOrEmpty(value))) return value;
            }
            return null;
        }

        public static (string left, string right) Splice(this string s, char separator)
        {
            var splice = s.Split(new char[] { separator }, count: 2);
            var left = splice.Length >= 1 ? splice[0] : null;
            var right = splice.Length >= 2 ? splice[1] : null;
            return (left, right);
        }

        public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Dictionary<TKey, TValue> other)
        {
            foreach(var item in other)
            {
                dictionary.Add(item.Key, item.Value);
            }
        }

        public static bool IsValidUrl(string source)
        {
            Uri uriResult;
            return Uri.TryCreate(source, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static bool IsUrl(string pattern)
        {
            return pattern.StartsWith("http");
        }
        
    }
}
