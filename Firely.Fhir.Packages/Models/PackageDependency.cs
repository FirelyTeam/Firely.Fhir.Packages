#nullable enable

using System.Collections.Generic;

namespace Firely.Fhir.Packages
{
    /// <summary>
    /// A packageDependency defines a version range for a specific package. If you want to target a very specific package version, use PackageReference
    /// A PackageDependency is used in a Manifest, while the PackageReference is used in a Context or Closure.
    /// </summary>
    public struct PackageDependency
    {
        public string Name;
        public string Range;  // 3.x, 3.1 - 3.3, 1.1 | 1.2

        /// <summary>
        /// Initializes a new package dependency
        /// </summary>
        /// <param name="name">name of the package</param>
        /// <param name="range">the version range for a specific package, when no version range is specified, the dependency will be on "latest"</param>
        public PackageDependency(string name, string? range = null)
        {
            this.Name = name;
            this.Range = range ?? "latest";
        }

        /// <summary>
        /// Converts a <see cref="KeyValuePair"/> of two strings to a <see cref="PackageDependency" /> where the key is the package name and the value is the version range
        /// </summary>
        /// <param name="pair"><see cref="KeyValuePair"/> defining a package dependency where the key is the package name and the value is the version range</param>
        public static implicit operator PackageDependency(KeyValuePair<string, string?> pair)
        {
            return new PackageDependency(pair.Key, pair.Value);
        }

        /// <summary>
        /// Converts a <see cref="string"/> to a new <see cref="PackageDependency" />, the string defines the name package name, the version range is null
        /// </summary>
        /// <param name="reference">a <see cref="string"/> defining a package name of a new package dependency</param>
        public static implicit operator PackageDependency(string reference)
        {
            var splitDependency = reference.Split('@');
            if (splitDependency.Length == 1)
                return new PackageDependency(reference, null);
            else
            {
                var versionDep = splitDependency[1];
                return new PackageDependency(splitDependency[0], versionDep);
            }
        }

        /// <summary>
        /// Converts a <see cref="PackageDependency"/> to a <see cref="string"/> including the package name and the version
        /// </summary>
        /// <returns>A <see cref="string"/> including the package name and the version</returns>
        public override string ToString()
        {
            return $"{Name} ({Range})";
        }
    }
}

#nullable restore
