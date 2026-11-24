/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Firely.Fhir.Packages;

internal class IntegerCanonicalVersionResolver : ICanonicalVersionResolver
{
    public VersionAlgorithm Algorithm => VersionAlgorithm.Integer;

    public bool Fits(IReadOnlyList<string> versions) => versions.All(v => v.Length > 0 && v.All(IsAsciiDigit));

    public int Compare(string a, string b) => CompareDigits(a.AsSpan(), b.AsSpan());

    internal static bool IsAsciiDigit(char c) => c is >= '0' and <= '9';

    // Compares ASCII digit runs of any length as numbers; leading zeros are insignificant.
    internal static int CompareDigits(ReadOnlySpan<char> a, ReadOnlySpan<char> b)
    {
        a = a.TrimStart('0');
        b = b.TrimStart('0');
        return a.Length != b.Length ? a.Length.CompareTo(b.Length) : Math.Sign(a.SequenceCompareTo(b));
    }
}
