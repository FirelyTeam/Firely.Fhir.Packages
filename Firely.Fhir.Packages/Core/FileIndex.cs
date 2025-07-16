/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace Firely.Fhir.Packages
{

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
            if (string.IsNullOrEmpty(version))
            {
                return this.FirstOrDefault(r => r.Canonical == canonical);
            }

            // First try exact match
            var exactMatch = this.FirstOrDefault(r => r.Canonical == canonical && r.Version == version);
            if (exactMatch != null)
            {
                return exactMatch;
            }

            // If no exact match, try partial version matching
            return this.FirstOrDefault(r => r.Canonical == canonical && IsVersionMatch(version, r.Version));
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
            if (string.IsNullOrEmpty(version))
            {
                var candidates = this.Where(r => r.Canonical == canonical);
                return candidates.Count() > 1 ? resolveFromMultipleCandidates(candidates) : candidates.SingleOrDefault();
            }

            // First try exact match
            var exactCandidates = this.Where(r => r.Canonical == canonical && r.Version == version);
            if (exactCandidates.Any())
            {
                return exactCandidates.Count() > 1 ? resolveFromMultipleCandidates(exactCandidates) : exactCandidates.Single();
            }

            // If no exact match, try partial version matching
            var partialCandidates = this.Where(r => r.Canonical == canonical && IsVersionMatch(version, r.Version));
            return partialCandidates.Count() > 1 ? resolveFromMultipleCandidates(partialCandidates) : partialCandidates.SingleOrDefault();
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

        private static PackageFileReference resolveFromMultipleCandidates(IEnumerable<PackageFileReference> candidates)
        {
            //first check which has the file has the highest version
            List<PackageFileReference> highestVersionedFiles = filterOnHighestVersions(candidates);

            if (highestVersionedFiles.Count == 1)
            {
                return highestVersionedFiles[0];
            }

            //If there are multiple, check if they have a snapshot or expansion, prefer those.
            else
            {
                candidates = filterOnSnapshotOrExpansions(candidates);
                return candidates.First();
            }
        }

        private static List<PackageFileReference> filterOnHighestVersions(IEnumerable<PackageFileReference> candidates) => candidates
                                                .GroupBy(file => Version.TryParse(file.Version, out var result) ? result : new Version("0.0.0"))
                                                .OrderByDescending(group => group.Key)
                                                .First()
                                                .ToList();

        private static IEnumerable<PackageFileReference> filterOnSnapshotOrExpansions(IEnumerable<PackageFileReference> candidates)
        {
            var snapshotsOrExpansions = candidates.Where(c => c.HasSnapshot == true || c.HasExpansion == true);
            if (snapshotsOrExpansions.Any())
            {
                candidates = snapshotsOrExpansions;
            }

            return candidates;
        }

        /// <summary>
        /// Determines if a partial version matches a full version according to FHIR canonical matching rules.
        /// For example, "1.5" should match "1.5.0", "1.5.1", etc.
        /// </summary>
        /// <param name="requestedVersion">The version being requested (may be partial)</param>
        /// <param name="candidateVersion">The actual version to check against</param>
        /// <returns>True if the versions match according to FHIR rules</returns>
        private static bool IsVersionMatch(string? requestedVersion, string? candidateVersion)
        {
            if (string.IsNullOrEmpty(requestedVersion) || string.IsNullOrEmpty(candidateVersion))
            {
                return false;
            }

            // Exact match
            if (requestedVersion == candidateVersion)
            {
                return true;
            }

            // Partial version matching: requested version should be a prefix of candidate version followed by a dot
            // e.g., "1.5" matches "1.5.0", "1.5.1", but not "1.50" or "1.51"
            return candidateVersion!.StartsWith(requestedVersion! + ".");
        }
    }
}

#nullable restore
