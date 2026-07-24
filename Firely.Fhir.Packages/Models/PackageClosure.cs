/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


#nullable enable

using SemanticVersioning;
using System.Collections.Generic;

namespace Firely.Fhir.Packages
{
    /// <summary>
    /// Package lock file
    /// </summary>
    /// <remarks>
    /// Create a package closure
    /// </remarks>
    /// <param name="conflictResolution">
    /// How to handle adding a package whose name already exists in the closure.
    /// Defaults to <see cref="ConflictResolutionStrategy.HighestWins"/>.
    /// </param>
    public class PackageClosure(ConflictResolutionStrategy conflictResolution = ConflictResolutionStrategy.HighestWins)
    {
        /// <summary>
        /// Whether the lock is complete
        /// </summary>
        public bool Complete => Missing.Count == 0;

        /// <summary>
        /// Package references currently inside the lock file
        /// </summary>
        public List<PackageReference> References = new();

        /// <summary>
        /// Missing package dependencies
        /// </summary>
        public List<PackageDependency> Missing = new();

        /// <summary>
        /// Strategy used to resolve a version conflict when adding a package reference
        /// whose name is already present in the closure.
        /// </summary>
        public ConflictResolutionStrategy ConflictResolution { get; } = conflictResolution;

        /// <summary>
        /// Add a package reference to the lock file
        /// </summary>
        /// <param name="reference">package reference to be added</param>
        /// <returns>Whether the package reference is successfully added</returns>
        public bool Add(PackageReference reference)
        {
            // An aliased reference (npm-style 'alias@npm:name') is an explicit request for this specific
            // version, so it is always kept regardless of the conflict-resolution strategy - the same
            // addIfNew used by AcceptMultiple below, since neither ever collapses/evicts, only rejects an
            // exact duplicate.
            if (reference.Alias is not null) return addIfNew(reference);

            return ConflictResolution switch
            {
                ConflictResolutionStrategy.HighestWins => addHighestWins(reference),
                ConflictResolutionStrategy.AcceptMultiple => addIfNew(reference),
                _ => throw new System.NotImplementedException(
                    $"No implementation for conflict resolution strategy '{ConflictResolution}'.")
            };
        }

        private bool addIfNew(PackageReference reference)
        {
            if (exists(reference.Name, reference.Version)) return false;

            References.Add(reference);
            return true;
        }

        private bool addHighestWins(PackageReference reference)
        {
            if (exists(reference.Name, reference.Version)) return false;

            if (findNonAliased(reference.Name, out var existing))
            {
                var highest = PackageClosure.highest(reference, existing);

                if (highest != existing)
                {
                    References.Remove(existing);
                    References.Add(highest);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                References.Add(reference);
                return true;
            }
        }

        private bool findNonAliased(string? pkgname, out PackageReference reference)
        {
            foreach (var refx in References)
            {
                if (refx.Alias is null && string.Compare(refx.Name, pkgname, ignoreCase: true) == 0)
                {
                    reference = refx;
                    return true;
                }
            }
            reference = default;
            return false;
        }

        /// <summary>
        /// Whether a package with the given name (case-insensitive) and version is already present.
        /// </summary>
        private bool exists(string? name, string? version)
        {
            foreach (var refx in References)
            {
                if (string.Compare(refx.Name, name, ignoreCase: true) == 0 && refx.Version == version)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Find a package by name in the closure.
        /// </summary>
        /// <remarks>
        /// The closure may hold more than one version of the same package name - under
        /// <see cref="ConflictResolutionStrategy.AcceptMultiple"/>, or via an npm-style alias, which always
        /// coexists regardless of strategy. Insertion order carries no meaning and cannot be predicted, so
        /// this method picks deterministically instead: it returns the highest version among all matching
        /// entries, aliased or not - the same choice <see cref="ConflictResolutionStrategy.HighestWins"/> would
        /// have made if it applied to every entry. Use <see cref="FindAll"/> to retrieve every matching entry
        /// rather than just one.
        /// </remarks>
        /// <param name="pkgname">package name to be found</param>
        /// <param name="reference">package reference of the found package</param>
        /// <returns>whether the package was found</returns>
        public bool Find(string? pkgname, out PackageReference reference)
        {
            PackageReference? highest = null;
            foreach (var refx in References)
            {
                if (string.Compare(refx.Name, pkgname, ignoreCase: true) != 0) continue;
                highest = highest is null || isHigherVersion(refx.Version, highest.Value.Version) ? refx : highest;
            }

            if (highest is not null)
            {
                reference = highest.Value;
                return true;
            }

            reference = default;
            return false;
        }

        /// <summary>
        /// Returns every reference in the closure matching the given package name (case-insensitive). There
        /// may be more than one - under <see cref="ConflictResolutionStrategy.AcceptMultiple"/>, or when
        /// npm-style aliases are present alongside a plain version.
        /// </summary>
        /// <param name="pkgname">package name to match</param>
        public IEnumerable<PackageReference> FindAll(string? pkgname)
        {
            foreach (var refx in References)
            {
                if (string.Compare(refx.Name, pkgname, ignoreCase: true) == 0)
                    yield return refx;
            }
        }

        internal void AddMissing(PackageDependency dependency)
        {
            // See Add(PackageReference): an aliased dependency is always kept regardless of strategy - the
            // same addMissingIfNew used by AcceptMultiple below.
            if (dependency.Alias is not null)
            {
                addMissingIfNew(dependency);
                return;
            }

            switch (ConflictResolution)
            {
                case ConflictResolutionStrategy.HighestWins:
                    addMissingHighestWins(dependency);
                    break;
                case ConflictResolutionStrategy.AcceptMultiple:
                    addMissingIfNew(dependency);
                    break;
                default:
                    throw new System.NotImplementedException(
                        $"No implementation for conflict resolution strategy '{ConflictResolution}'.");
            }
        }

        private void addMissingIfNew(PackageDependency dependency)
        {
            if (existsInMissing(dependency.Name, dependency.Range)) return;

            Missing.Add(dependency);
        }

        private void addMissingHighestWins(PackageDependency dependency)
        {
            if (existsInMissing(dependency.Name, dependency.Range)) return;

            var index = Missing.FindIndex(m => m.Alias is null && string.Compare(m.Name, dependency.Name, ignoreCase: true) == 0);

            if (index < 0)
            {
                Missing.Add(dependency);
                return;
            }

            Missing[index] = highest(dependency, Missing[index]);
        }

        private bool existsInMissing(string? name, string? range)
        {
            foreach (var existing in Missing)
            {
                if (string.Compare(existing.Name, name, ignoreCase: true) == 0 && existing.Range == range)
                    return true;
            }
            return false;
        }

        private static PackageReference highest(PackageReference A, PackageReference B) => isHigherVersion(A.Version, B.Version) ? A : B;

        private static PackageDependency highest(PackageDependency A, PackageDependency B) => isHigherVersion(A.Range, B.Range) ? A : B;

        private static bool isHigherVersion(string? candidate, string? existing)
        {
            var candidateVersion = Version.TryParse(candidate, out var c) ? c : new Version("0.0.0");
            var existingVersion = Version.TryParse(existing, out var e) ? e : new Version("0.0.0");

            return candidateVersion > existingVersion;
        }

    }

}

#nullable restore