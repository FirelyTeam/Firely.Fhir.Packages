/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */

using System.Collections.Generic;
using System.Linq;

namespace Firely.Fhir.Packages;

internal static class CanonicalReferenceHandling
{
    // Tried in order; the alphabetical resolver matches anything, so it stays last.
    private static readonly ICanonicalVersionResolver[] VersionResolvers =
    [
        new SemVerCanonicalVersionResolver(),
        new NumericCanonicalVersionResolver(),
        new DateCanonicalVersionResolver(),
        new AlphabeticalCanonicalVersionResolver()
    ];

    internal static ICanonicalVersionResolver ResolverFor(IReadOnlyList<string> versions) =>
        VersionResolvers.First(r => r.MatchThisAlgorithm(versions));
}
