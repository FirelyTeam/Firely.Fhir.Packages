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
    public PackageFileReference? ResolveCanonical(string canonical, string? version = null)
    {
        if (version is null || string.IsNullOrEmpty(version))
        {
            return this.FirstOrDefault(r => r.Canonical == canonical);
        }

        return this.FirstOrDefault(r => r.Canonical == canonical && Canonical.MatchesVersion(r.Version,version));
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
        var candidates = version is null || string.IsNullOrEmpty(version)
            ? this.Where(r => r.Canonical == canonical).ToList()
            : this.Where(r => r.Canonical == canonical && Canonical.MatchesVersion(r.Version, version)).ToList();

        return candidates.Count switch
        {
            > 1 => resolveFromMultipleCandidates(candidates),
            1 => candidates.Single(),
            _ => null
        };
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
    /// Non-parseable version strings (e.g. date-based) are treated as <c>0.0.0</c>.
    /// </summary>
    public static T SelectBestCandidate<T>(IEnumerable<T> candidates, Func<T, string?> getVersion) =>
        SelectBestCandidate(candidates, getVersion, _ => false);

    /// <summary>
    /// Selects the best candidate from a list of items by choosing the one with the highest resource version.
    /// When multiple items share the highest version, <paramref name="preferred"/> is used as a tie-breaker.
    /// Non-parseable version strings (e.g. date-based) are treated as <c>0.0.0</c>.
    /// </summary>
    public static T SelectBestCandidate<T>(IEnumerable<T> candidates, Func<T, string?> getVersion, Func<T, bool> preferred)
    {
        var highestVersioned = candidates
            .GroupBy(item => Version.TryParse(getVersion(item), out var v) ? v : new Version(0, 0, 0))
            .OrderByDescending(g => g.Key)
            .First()
            .ToList();

        if (highestVersioned.Count == 1)
            return highestVersioned[0];

        var preferredItems = highestVersioned.Where(preferred).ToList();
        return preferredItems.Count > 0 ? preferredItems[0] : highestVersioned[0];
    }

    private static PackageFileReference resolveFromMultipleCandidates(List<PackageFileReference> candidates) =>
        SelectBestCandidate(candidates, f => f.Version, f => f.HasSnapshot == true || f.HasExpansion == true);
}