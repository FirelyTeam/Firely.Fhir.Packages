/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


#nullable enable

using Hl7.Fhir.Model;
using Hl7.Fhir.Utility;
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
        /// </summary>
        /// <param name="canonical">canonical URI used to identify the artifact</param>
        /// <param name="version">version of the artifact</param>
        /// <param name="fhirVersion">Only resolve files that conform to this FHIR version</param>
        /// <returns>First file found with a specific canonical URI and optional version</returns>
        public PackageFileReference? ResolveCanonical(string canonical, string? version = null, FHIRVersion? fhirVersion = null)
        {
            var candidates = !string.IsNullOrEmpty(version)
                ? this.Where(r => r.Canonical == canonical && r.Version == version)
                : this.Where(r => r.Canonical == canonical);

            if (fhirVersion.GetLiteral() is { } fhirVersionString)
            {
                return candidates.Where(r => r.FhirVersion == fhirVersionString).FirstOrDefault();
            }
            else
            {
                return candidates.FirstOrDefault();
            }
        }

        /// <summary>
        /// Returns the best candidate found with a specific canonical URI and optional version.
        /// </summary>
        /// <param name="canonical">canonical URI used to identify the artifact</param>
        /// <param name="version">version of the artifact</param>
        /// <param name="fhirVersion">Only resolve files that conform to this FHIR version</param>
        /// <returns>Returns the best candidate found with a specific canonical URI and optional version.</returns>
        public PackageFileReference? ResolveBestCandidateByCanonical(string canonical, string? version = null, FHIRVersion? fhirVersion = null)
        {
            var candidates = !string.IsNullOrEmpty(version)
                ? this.Where(r => r.Canonical == canonical && r.Version == version)
                : this.Where(r => r.Canonical == canonical);


            if (fhirVersion.GetLiteral() is { } fhirVersionString)
            {
                candidates = candidates.Where(r => r.FhirVersion == fhirVersionString);
            }

            return candidates.Count() > 1 ? resolveFromMultipleCandidates(candidates) : candidates.SingleOrDefault();
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
    }
}

#nullable restore
