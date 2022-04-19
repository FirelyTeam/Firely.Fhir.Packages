/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


#nullable enable

using System;
using System.Collections.Generic;

namespace Firely.Fhir.Packages
{
    internal static class Helpers
    {
        // Missing in netstandard.
        internal static string[] Split(this string s, string separator)
        {
            return s.Split(new string[] { separator }, StringSplitOptions.None);
        }

        internal static (string? left, string? right) Splice(this string s, char separator)
        {
            var splice = s.Split(new char[] { separator }, count: 2);
            var left = splice.Length >= 1 ? splice[0] : null;
            var right = splice.Length >= 2 ? splice[1] : null;
            return (left, right);
        }


        internal static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Dictionary<TKey, TValue> other) where TKey : notnull
        {
            foreach (var item in other)
            {
                dictionary.Add(item.Key, item.Value);
            }
        }

        internal static bool IsValidUrl(string source)
        {
            return Uri.TryCreate(source, UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        internal static bool IsUrl(string pattern)
        {
            return pattern.StartsWith("http");
        }
    }
}

#nullable restore