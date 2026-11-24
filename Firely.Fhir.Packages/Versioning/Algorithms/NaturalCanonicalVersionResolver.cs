/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */

using System;
using System.Collections.Generic;

namespace Firely.Fhir.Packages;

internal class NaturalCanonicalVersionResolver : ICanonicalVersionResolver
{
    public VersionAlgorithm Algorithm => VersionAlgorithm.Natural;

    public bool Fits(IReadOnlyList<string> versions) => true;

    // Compares runs of ASCII digits numerically and other runs case-insensitively; a digit run ranks below text, a shorter prefix ranks lower.
    public int Compare(string a, string b)
    {
        int i = 0, j = 0;
        while (i < a.Length && j < b.Length)
        {
            var aRun = nextRun(a, ref i, out var aDigits);
            var bRun = nextRun(b, ref j, out var bDigits);

            var result = (aDigits, bDigits) switch
            {
                (true, true) => IntegerCanonicalVersionResolver.CompareDigits(aRun, bRun),
                (true, false) => -1,
                (false, true) => 1,
                _ => Math.Sign(aRun.CompareTo(bRun, StringComparison.OrdinalIgnoreCase))
            };
            if (result != 0) return result;
        }

        return (i < a.Length).CompareTo(j < b.Length);
    }

    private static ReadOnlySpan<char> nextRun(string s, ref int pos, out bool digits)
    {
        var start = pos;
        digits = IntegerCanonicalVersionResolver.IsAsciiDigit(s[pos]);
        while (pos < s.Length && IntegerCanonicalVersionResolver.IsAsciiDigit(s[pos]) == digits) pos++;
        return s.AsSpan(start, pos - start);
    }
}
