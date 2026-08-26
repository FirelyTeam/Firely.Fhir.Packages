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
    public static class PackageReferenceExtensions
    {
        internal static List<PackageReference> ToPackageReferences(this IEnumerable<LockFileDependency> dependencies)
        {
            var list = new List<PackageReference>();
            foreach (var dependency in dependencies)
            {
                if (dependency.Name is null) continue;
                // Constructed, not object-initialised: the constructor is what lowercases Name.
                list.Add(new PackageReference(dependency.Name, dependency.Version) { Alias = dependency.Alias });
            }
            return list;
        }

        internal static List<PackageDependency> ToPackageDependencies(this IEnumerable<LockFileDependency> dependencies)
        {
            var list = new List<PackageDependency>();
            foreach (var dependency in dependencies)
            {
                if (dependency.Name is null) continue;
                list.Add(new PackageDependency(dependency.Name, dependency.Version) { Alias = dependency.Alias });
            }
            return list;
        }

        internal static List<LockFileDependency> ToLockFileDependencies(this IEnumerable<PackageReference> references)
        {
            var list = new List<LockFileDependency>();
            foreach (var reference in references)
            {
                if (reference.Name is null) continue;
                list.Add(new LockFileDependency(reference.Name, reference.Version, reference.Alias));
            }
            return list;
        }

        internal static List<LockFileDependency> ToLockFileDependencies(this IEnumerable<PackageDependency> dependencies)
        {
            var list = new List<LockFileDependency>();
            foreach (var dependency in dependencies)
            {
                list.Add(new LockFileDependency(dependency.Name, dependency.Range, dependency.Alias));
            }
            return list;
        }

        /// <summary>
        /// Get dependencies from a package manifest
        /// </summary>
        /// <param name="manifest">Package manifest from which the dependencies are being retrieved</param>
        /// <returns>A list of package dependencies</returns>
        public static IEnumerable<PackageDependency> GetDependencies(this PackageManifest manifest)
        {
            if (manifest.Dependencies is null) yield break;

            foreach (PackageDependency dep in manifest.Dependencies)
            {
                yield return dep;
            }
        }

        /// <summary>
        /// Get the NPM name of a package
        /// </summary>
        /// <param name="reference">Package of which the NPM name is to be retrieved</param>
        /// <returns>NPM name of the package</returns>
        public static string? GetNpmName(this PackageReference reference)
        {
            return (reference.Scope == null) ? reference.Name : $"@{reference.Scope}%2F{reference.Name}";
        }
    }
}

#nullable restore