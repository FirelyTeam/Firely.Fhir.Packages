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
            return ConflictResolution switch
            {
                ConflictResolutionStrategy.HighestWins => addHighestWins(reference),
                ConflictResolutionStrategy.AcceptMultiple => addAcceptMultiple(reference),
                _ => throw new System.NotImplementedException(
                    $"No implementation for conflict resolution strategy '{ConflictResolution}'.")
            };
        }

        private bool addHighestWins(PackageReference reference)
        {
            if (Find(reference.Name, out var existing))
            {
                if (existing == reference) return false;

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

        private bool addAcceptMultiple(PackageReference reference)
        {
            if (exists(reference.Name, reference.Version)) return false;

            References.Add(reference);
            return true;
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

        private static PackageReference highest(PackageReference A, PackageReference B)
        {
            var versionA = Version.TryParse(A.Version, out var resultA) ? resultA : new Version("0.0.0");
            var versionB = Version.TryParse(B.Version, out var resultB) ? resultB : new Version("0.0.0");
            var highest = (versionA > versionB) ? A : B;

            return highest;
        }

        /// <summary>
        /// Find a package name in the lock file
        /// </summary>
        /// <param name="pkgname">package name to be found</param>
        /// <param name="reference">package reference of the found package</param>
        /// <returns>whether the package was found</returns>
        public bool Find(string? pkgname, out PackageReference reference)
        {
            foreach (var refx in References)
            {
                if (string.Compare(refx.Name, pkgname, ignoreCase: true) == 0)
                {
                    reference = refx;
                    return true;
                }
            }
            reference = default;
            return false;
        }

        internal void AddMissing(PackageDependency dependency)
        {
            switch (ConflictResolution)
            {
                case ConflictResolutionStrategy.HighestWins:
                    addMissingHighestWins(dependency);
                    break;
                case ConflictResolutionStrategy.AcceptMultiple:
                    addMissingAcceptMultiple(dependency);
                    break;
                default:
                    throw new System.NotImplementedException(
                        $"No implementation for conflict resolution strategy '{ConflictResolution}'.");
            }
        }

        private void addMissingHighestWins(PackageDependency dependency)
        {
            // Keep a single entry per package name (highest range), so a HighestWins closure holds one
            // version per name in Missing just as it does in References.
            var index = Missing.FindIndex(m => string.Compare(m.Name, dependency.Name, ignoreCase: true) == 0);
            if (index < 0)
            {
                Missing.Add(dependency);
                return;
            }

            Missing[index] = highest(dependency, Missing[index]);
        }

        private void addMissingAcceptMultiple(PackageDependency dependency)
        {
            if (!Missing.Contains(dependency)) Missing.Add(dependency);
        }

        private static PackageDependency highest(PackageDependency A, PackageDependency B)
        {
            var versionA = Version.TryParse(A.Range, out var resultA) ? resultA : new Version("0.0.0");
            var versionB = Version.TryParse(B.Range, out var resultB) ? resultB : new Version("0.0.0");
            return (versionA > versionB) ? A : B;
        }

    }

}

#nullable restore