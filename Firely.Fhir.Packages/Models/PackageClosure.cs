/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


#nullable enable

using System.Collections.Generic;

namespace Firely.Fhir.Packages
{
    /// <summary>
    /// Package lock file
    /// </summary>
    /// <remarks>
    /// A closure can hold multiple versions of the same package at once: a FHIR dependency graph can
    /// legitimately require several versions simultaneously - e.g. via canonical references pinned to a
    /// specific version, or reuse-wrapper packages - so no version is ever silently discarded.
    /// </remarks>
    public class PackageClosure
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
        /// Add a package reference to the lock file. Multiple versions of the same package can coexist;
        /// only an exact duplicate (same name and version) is rejected.
        /// </summary>
        /// <param name="reference">package reference to be added</param>
        /// <returns>Whether the package reference is successfully added</returns>
        public bool Add(PackageReference reference)
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
                if (sameName(refx.Name, name) && refx.Version == version)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Compares two package names. Package names are case-insensitive, and are compared using
        /// ordinal rules: culture-aware casing would, for example, consider "fhir" and "FHIR" different
        /// names under Turkish/Azeri cultures because of their dotless-i casing rules.
        /// </summary>
        private static bool sameName(string? left, string? right)
            => string.Equals(left, right, System.StringComparison.OrdinalIgnoreCase);

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
                if (sameName(refx.Name, pkgname))
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
            foreach (var existing in Missing)
            {
                if (sameName(existing.Name, dependency.Name)
                    && existing.Range == dependency.Range)
                {
                    return;
                }
            }

            Missing.Add(dependency);
        }
    }

}

#nullable restore