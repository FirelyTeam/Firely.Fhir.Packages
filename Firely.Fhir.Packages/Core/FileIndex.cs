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
        /// </summary>
        /// <param name="canonical">canonical URI used to identify the artifact</param>
        /// <param name="version">version of the artifact</param>
        /// <returns>First file found with a specific canonical URI and optional version</returns>
        public PackageFileReference? ResolveCanonical(string canonical, string? version = null)
        {
            return !string.IsNullOrEmpty(version)
                ? this.FirstOrDefault(r => r.Canonical == canonical && r.Version == version)
                : this.FirstOrDefault(r => r.Canonical == canonical);
        }

        /// <summary>
        /// Returns the best candidate found with a specific canonical URI and optional version.
        /// </summary>
        /// <param name="canonical">canonical URI used to identify the artifact</param>
        /// <param name="version">version of the artifact</param>
        /// <returns>Returns the best candidate found with a specific canonical URI and optional version.</returns>
        public PackageFileReference? ResolveBestCandidateByCanonical(string canonical, string? version = null)
        {
            var candidates = !string.IsNullOrEmpty(version)
                ? this.Where(r => r.Canonical == canonical && r.Version == version)
                : this.Where(r => r.Canonical == canonical);

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
            candidates = candidates.Where(c => c.HasSnapshot == true || c.HasExpansion == true);
            return candidates.Count() == 1
                ? candidates.First()
                : throw new InvalidOperationException("Found multiple conflicting conformance resources with the same canonical url identifier.");
        }



    }

}

#nullable restore
