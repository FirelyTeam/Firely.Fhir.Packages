/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Firely.Fhir.Packages;

/// <summary>
/// Looks up the <see cref="ICanonicalVersionResolver"/> that orders the versions of a canonical resource.
/// </summary>
public static class CanonicalReferenceHandling
{
    /// <summary>
    /// The resolver for each <see cref="VersionAlgorithm"/>, one entry per value; the dictionary is read-only.
    /// </summary>
    public static IReadOnlyDictionary<VersionAlgorithm, ICanonicalVersionResolver> VersionResolvers { get; } =
        new ReadOnlyDictionary<VersionAlgorithm, ICanonicalVersionResolver>(new ICanonicalVersionResolver[]
        {
            new SemVerCanonicalVersionResolver(),
            new IntegerCanonicalVersionResolver(),
            new AlphabeticalCanonicalVersionResolver(),
            new DateCanonicalVersionResolver(),
            new NaturalCanonicalVersionResolver()
        }.ToDictionary(r => r.Algorithm));

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

    /// <summary>
    /// Parses a FHIR version-algorithm code: exactly one of "semver", "integer", "alpha", "date" or "natural", case-sensitive.
    /// </summary>
    /// <param name="code">The code to parse, or null.</param>
    /// <param name="algorithm">The parsed algorithm, or the default value when parsing fails.</param>
    /// <returns>True when <paramref name="code"/> is a known code; false when it is null or unknown.</returns>
    public static bool TryParseVersionAlgorithm(string? code, out VersionAlgorithm algorithm)
    {
        algorithm = default;
        return code is not null && AlgorithmCodes.TryGetValue(code, out algorithm);
    }

    /// <summary>
    /// Infers the resolver for a set of versions: SemVer when all fit SemVer, else Date when all fit Date, else Natural.
    /// </summary>
    /// <param name="versions">The versions to order; every element must be non-null.</param>
    /// <exception cref="ArgumentNullException"><paramref name="versions"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="versions"/> contains a null element.</exception>
    public static ICanonicalVersionResolver ResolverFor(IReadOnlyList<string> versions)
    {
        if (versions is null) throw new ArgumentNullException(nameof(versions));
        if (versions.Any(v => v is null)) throw new ArgumentException("The versions must not contain null.", nameof(versions));
        return InferenceOrder.Select(a => VersionResolvers[a]).First(r => r.Fits(versions));
    }

    // Uses the declared algorithm when all declarations agree on a known code that fits every version; otherwise infers.
    internal static ICanonicalVersionResolver ResolverFor(IReadOnlyList<string> versions, IEnumerable<string?> declaredAlgorithms)
    {
        var declared = declaredAlgorithms.Where(d => !string.IsNullOrEmpty(d)).Distinct(System.StringComparer.Ordinal).Take(2).ToList();
        return declared.Count == 1 && TryParseVersionAlgorithm(declared[0], out var algorithm) && VersionResolvers[algorithm].Fits(versions)
            ? VersionResolvers[algorithm]
            : ResolverFor(versions);
    }
}
