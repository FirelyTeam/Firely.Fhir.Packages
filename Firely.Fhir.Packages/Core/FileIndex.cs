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

    private static PackageFileReference resolveFromMultipleCandidates(List<PackageFileReference> candidates)
    {
        //first check which has the file has the highest version
        var highestVersionedFiles = filterOnHighestVersions(candidates);

        if (highestVersionedFiles.Count == 1)
            return highestVersionedFiles[0];

        //If there are multiple, check if they have a snapshot or expansion, prefer those.
        candidates = filterOnSnapshotOrExpansions(candidates);
        return candidates.First();
    }

    private static List<PackageFileReference> filterOnHighestVersions(List<PackageFileReference> candidates) => candidates
        .GroupBy(file => Version.TryParse(file.Version, out var result) ? result : new Version("0.0.0"))
        .OrderByDescending(group => group.Key)
        .First()
        .ToList();

    private static List<PackageFileReference> filterOnSnapshotOrExpansions(List<PackageFileReference> candidates)
    {
        var snapshotsOrExpansions = candidates.Where(c => c.HasSnapshot == true || c.HasExpansion == true).ToList();

        return snapshotsOrExpansions.Any() ? snapshotsOrExpansions : candidates;
    }
}