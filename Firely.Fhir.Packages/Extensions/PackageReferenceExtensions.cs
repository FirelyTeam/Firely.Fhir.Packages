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
using System.Linq;

namespace Firely.Fhir.Packages
{
    public static class PackageReferenceExtensions
    {
        internal static Dictionary<string, string?> ToDictionary(this IEnumerable<PackageReference> references)
        {
            var dict = new Dictionary<string, string?>();
            foreach (var reference in references.Where(r => r.Name is not null))
            {
                if (dict.ContainsKey(reference.Name!))
                {
                    // If the key already exists, keep the highest version
                    var existingVersion = dict[reference.Name!];
                    var newVersion = reference.Version;
                    
                    var versionExisting = Version.TryParse(existingVersion, out var resultExisting) ? resultExisting : new Version("0.0.0");
                    var versionNew = Version.TryParse(newVersion, out var resultNew) ? resultNew : new Version("0.0.0");
                    
                    if (versionNew > versionExisting)
                    {
                        dict[reference.Name!] = newVersion;
                    }
                }
                else
                {
                    dict.Add(reference.Name!, reference.Version);
                }
            }
            return dict;
        }

        internal static Dictionary<string, string?> ToDictionary(this IEnumerable<PackageDependency> references)
        {
            var dict = new Dictionary<string, string?>();
            foreach (var reference in references)
            {
                // If the key already exists, keep the first occurrence
                // (since Range is a version range, not a specific version)
                if (!dict.ContainsKey(reference.Name))
                {
                    dict.Add(reference.Name, reference.Range);
                }
            }
            return dict;
        }

        internal static List<PackageReference> ToPackageReferences(this Dictionary<string, string?> dict)
        {
            var list = new List<PackageReference>();

            foreach (var item in dict)
            {
                list.Add(item); // implicit converion
            }
            return list;
        }

        internal static List<PackageDependency> ToPackageDependencies(this Dictionary<string, string?> dict)
        {
            var list = new List<PackageDependency>();
            foreach (var item in dict)
            {
                list.Add(item); // implicit converion
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