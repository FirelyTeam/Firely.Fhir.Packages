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
    internal static readonly IReadOnlyDictionary<VersionAlgorithm, ICanonicalVersionResolver> VersionResolvers =
        new ICanonicalVersionResolver[]
        {
            new SemVerCanonicalVersionResolver(),
            new IntegerCanonicalVersionResolver(),
            new AlphabeticalCanonicalVersionResolver(),
            new DateCanonicalVersionResolver(),
            new NaturalCanonicalVersionResolver()
        }.ToDictionary(r => r.Algorithm);

    // Inference order; the natural resolver matches anything, so it stays last.
    private static readonly VersionAlgorithm[] InferenceOrder = [VersionAlgorithm.SemVer, VersionAlgorithm.Date, VersionAlgorithm.Natural];

    // FHIR version-algorithm codes (http://hl7.org/fhir/version-algorithm).
    private static readonly Dictionary<string, VersionAlgorithm> AlgorithmCodes = new()
    {
        ["semver"] = VersionAlgorithm.SemVer,
        ["integer"] = VersionAlgorithm.Integer,
        ["alpha"] = VersionAlgorithm.Alpha,
        ["date"] = VersionAlgorithm.Date,
        ["natural"] = VersionAlgorithm.Natural
    };

    internal static ICanonicalVersionResolver ResolverFor(IReadOnlyList<string> versions) =>
        InferenceOrder.Select(a => VersionResolvers[a]).First(r => r.MatchThisAlgorithm(versions));

    // Uses the declared algorithm when all declarations agree on a known code that fits every version; otherwise infers.
    internal static ICanonicalVersionResolver ResolverFor(IReadOnlyList<string> versions, IEnumerable<string?> declaredAlgorithms)
    {
        var declared = declaredAlgorithms.Where(d => !string.IsNullOrEmpty(d)).Distinct(System.StringComparer.Ordinal).Take(2).ToList();
        return declared.Count == 1 && AlgorithmCodes.TryGetValue(declared[0]!, out var algorithm) && VersionResolvers[algorithm].MatchThisAlgorithm(versions)
            ? VersionResolvers[algorithm]
            : ResolverFor(versions);
    }
}
