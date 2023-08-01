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
        /// Add a package reference to the lock file
        /// </summary>
        /// <param name="reference">package reference to be added</param>
        /// <returns>Whether the package reference is successfully added</returns>
        public bool Add(PackageReference reference)
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


        private static PackageReference highest(PackageReference A, PackageReference B)
        {
            var versionA = new Version(A.Version);
            var versionB = new Version(B.Version);
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

        internal void AddMissing(PackageDependency reference)
        {
            if (!Missing.Contains(reference)) Missing.Add(reference);
        }

    }

}

#nullable restore