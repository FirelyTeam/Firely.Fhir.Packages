/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Firely.Fhir.Packages;

internal partial class DateCanonicalVersionResolver : ICanonicalVersionResolver
{
    private const string DateVersionPattern = @"^[0-9]{4}((-?)(0[1-9]|1[0-2])(\2(0[1-9]|[12][0-9]|3[01]))?)?\z";

#if NET7_0_OR_GREATER
    [GeneratedRegex(DateVersionPattern)]
    private static partial Regex DateVersion();
#else
    // netstandard2.1 has no regex source generator.
    private static readonly Regex DateVersionRegex = new(DateVersionPattern, RegexOptions.Compiled);

    private static Regex DateVersion() => DateVersionRegex;
#endif

    public VersionAlgorithm Algorithm => VersionAlgorithm.Date;

    public bool Fits(IReadOnlyList<string> versions) => versions.All(DateVersion().IsMatch);

    public int Compare(string a, string b) => string.CompareOrdinal(digitsOf(a), digitsOf(b));

    private static string digitsOf(string version) => new([.. version.Where(char.IsDigit)]);
}
