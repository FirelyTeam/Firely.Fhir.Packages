/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


#nullable enable

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Firely.Fhir.Packages
{
    internal static class UrlParameterExtensions
    {
        internal static void AddWhenValued(this NameValueCollection collection, string name, string? value)
        {
            if (!string.IsNullOrEmpty(value))
                collection.Add(name, value);
        }

        internal static string ToQueryString(this NameValueCollection collection)
        {
            var builder = new StringBuilder();

            bool first = true;

            foreach (var key in collection.AllKeys.Where(k => k is not null))
            {
                var values = collection.GetValues(key);

                if (values != null)
                {
                    foreach (var value in values)
                    {
                        if (!first)
                        {
                            builder.Append('&');
                        }

                        builder.AppendFormat("{0}={1}", Uri.EscapeDataString(key!), Uri.EscapeDataString(value));

                        first = false;
                    }
                }
            }

            return builder.ToString();
        }
    }
}

#nullable restore