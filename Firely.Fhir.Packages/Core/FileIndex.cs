/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Firely.Fhir.Packages;

public class FileIndex : List<PackageFileReference>
{
    public FileIndex()
    {
    }


    /// <summary>
    /// Returns the first file found with a specific canonical URI and optional version.
    /// Supports partial version matching according to FHIR canonical matching specification.
    /// </summary>
    /// <param name="canonical">canonical URI used to identify the artifact</param>
    /// <param name="version">version of the artifact</param>
    /// <returns>First file found with a specific canonical URI and optional version</returns>
    /// <remarks>
    /// "First" means first in index order, which follows the insertion order of the closure the index was
    /// built from. A closure can hold several versions of the same package, so the same canonical can occur
    /// at multiple resource versions and the one returned here is effectively arbitrary. Use
    /// <see cref="ResolveBestCandidateByCanonical(string, string?)"/> to get the highest version, or
    /// <see cref="ResolveAllCanonical(string, string?)"/> to inspect every match yourself.
    /// </remarks>
    [Obsolete("Returns an arbitrary match when the index contains a canonical at more than one version, " +
        "which is normal now that a closure can hold several versions of the same package. Use " +
        "ResolveBestCandidateByCanonical to get the highest version, or ResolveAllCanonical for every match.")]
    public PackageFileReference? ResolveCanonical(string canonical, string? version = null)
    {
        return findCandidates(canonical, version).FirstOrDefault();
    }

    /// <summary>
    /// Returns the best candidate found with a specific canonical URI and optional version.
    /// Supports partial version matching according to FHIR canonical matching specification.
    /// </summary>
    /// <param name="canonical">canonical URI used to identify the artifact</param>
    /// <param name="version">version of the artifact</param>
    /// <returns>Returns the best candidate found with a specific canonical URI and optional version.</returns>
    public PackageFileReference? ResolveBestCandidateByCanonical(string canonical, string? version = null)
    {
        var candidates = findCandidates(canonical, version).ToList();

        return candidates.Count switch
        {
            > 1 => resolveFromMultipleCandidates(candidates),
            1 => candidates.Single(),
            _ => null
        };
    }

    /// <summary>
    /// Returns every file found with a specific canonical URI and optional version, in index order.
    /// Supports partial version matching according to FHIR canonical matching specification.
    /// </summary>
    /// <param name="canonical">canonical URI used to identify the artifact</param>
    /// <param name="version">version of the artifact</param>
    /// <returns>All files matching the canonical URI and optional version; empty when there are none.</returns>
    /// <remarks>
    /// Because a closure can hold several versions of the same package, one canonical can legitimately
    /// resolve to more than one artifact. Use this when the caller needs to see all of them rather than
    /// have one picked for it.
    /// </remarks>
    public IReadOnlyList<PackageFileReference> ResolveAllCanonical(string canonical, string? version = null)
    {
        return [.. findCandidates(canonical, version)];
    }

    private IEnumerable<PackageFileReference> findCandidates(string canonical, string? version)
    {
        return string.IsNullOrEmpty(version)
            ? this.Where(r => r.Canonical == canonical)
            : this.Where(r => r.Canonical == canonical && Canonical.MatchesVersion(r.Version, version));
    }

    /// <summary>
    /// Adds the metadata of a artifact to the index of a package
    /// </summary>
    /// <param name="package">Reference to a specific version of a package</param>
    /// <param name="metadata">The added metadata of a artifact</param>
    public void Add(PackageReference package, ResourceMetadata metadata)
    {
        var reference = new PackageFileReference(metadata.FileName, metadata.FilePath) { Package = package };
        metadata.CopyTo(reference);
        Add(reference);
    }

    /// <summary>
    /// Selects the best candidate from a list of items by choosing the one with the highest resource version.
    /// When multiple items share the highest version, the first is returned.
    /// Versions are ordered by SemVer when all are SemVer, by date when all are dates, else alphabetically; a missing version ranks lowest.
    /// </summary>
    public static T SelectBestCandidate<T>(IEnumerable<T> candidates, Func<T, string?> getVersion) =>
        SelectBestCandidate(candidates, getVersion, _ => false);

    /// <summary>
    /// Selects the best candidate from a list of items by choosing the one with the highest resource version.
    /// When multiple items share the highest version, <paramref name="preferred"/> is used as a tie-breaker.
    /// Versions are ordered by SemVer when all are SemVer, by date when all are dates, else alphabetically; a missing version ranks lowest.
    /// </summary>
    public static T SelectBestCandidate<T>(IEnumerable<T> candidates, Func<T, string?> getVersion, Func<T, bool> preferred)
    {
        var items = candidates.ToList();
        var compare = versionComparison([.. items.Select(getVersion).Where(v => !string.IsNullOrEmpty(v)).Select(v => v!)]);
        var best = items.Aggregate((max, item) => compare(getVersion(item), getVersion(max)) > 0 ? item : max);
        var highestVersioned = items.Where(item => compare(getVersion(item), getVersion(best)) == 0).ToList();

        if (highestVersioned.Count == 1)
            return highestVersioned[0];

        var preferredItems = highestVersioned.Where(preferred).ToList();
        return preferredItems.Count > 0 ? preferredItems[0] : highestVersioned[0];
    }

    private static readonly Regex DateVersion = new(@"^\d{4}(-?\d{2}(-?\d{2})?)?$", RegexOptions.Compiled);

    // Mirrors Simplifier's CanonicalReferenceHandling.FindBestVersion: SemVer, else date, else alphabetical; missing lowest.
    private static Comparison<string?> versionComparison(List<string> versions)
    {
        Comparison<string> byValue =
            versions.All(v => SemanticVersioning.Version.TryParse(v, out _)) ? (a, b) => new SemanticVersioning.Version(a).CompareTo(new SemanticVersioning.Version(b))
            : versions.All(DateVersion.IsMatch) ? (a, b) => string.CompareOrdinal(digitsOf(a), digitsOf(b))
            : StringComparer.OrdinalIgnoreCase.Compare;

        return (a, b) => (string.IsNullOrEmpty(a), string.IsNullOrEmpty(b)) switch
        {
            (true, true) => 0,
            (true, false) => -1,
            (false, true) => 1,
            _ => byValue(a!, b!)
        };
    }

    private static string digitsOf(string version) => new([.. version.Where(char.IsDigit)]);

    private static PackageFileReference resolveFromMultipleCandidates(List<PackageFileReference> candidates) =>
        SelectBestCandidate(candidates, f => f.Version, f => f.HasSnapshot == true || f.HasExpansion == true);
}