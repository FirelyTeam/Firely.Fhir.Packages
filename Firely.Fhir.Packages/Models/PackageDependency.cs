#nullable enable

using System;
using System.Collections.Generic;

namespace Firely.Fhir.Packages
{
    /// <summary>
    /// A packageDependency defines a version range for a specific package. If you want to target a very specific package version, use PackageReference
    /// A PackageDependency is used in a Manifest, while the PackageReference is used in a Context or Closure.
    /// </summary>
    public struct PackageDependency
    {
        private string _name;

        /// <summary>
        /// The package name (always normalized to lowercase by the constructor).
        /// </summary>
        public string Name
        {
            readonly get => _name;
            [Obsolete("Setting Name directly bypasses lowercase normalization and can produce an invalid, inconsistent package dependency. Construct a new PackageDependency using the constructor instead, which normalizes the name.")]
            set => _name = value;
        }

        public string Range;  // 3.x, 3.1 - 3.3, 1.1 | 1.2

        /// <summary>
        /// Optional local alias, from an npm-style <c>alias@npm:name</c> dependency. When set, this dependency
        /// is an explicit request that should be kept even if other versions/ranges of the same package are present.
        /// <see cref="Name"/> always holds the real package name.
        public string? Alias;

        /// <summary>
        /// Initializes a new package dependency
        /// </summary>
        /// <param name="name">name of the package</param>
        /// <param name="range">the version range for a specific package, when no version range is specified, the dependency will be on "latest"</param>
        public PackageDependency(string name, string? range = null)
        {
            // FHIR/NPM package ids are lowercase. Normalise here so that the casing a caller
            // happens to use - typed on a command line, or published in someone else's
            // manifest - cannot leak into manifests, lock files or cache folder names, and
            // cannot make the same package compare unequal to itself.
            // Assigned directly to the backing field, bypassing the (obsolete) Name setter.
            _name = name?.ToLowerInvariant()!;
            this.Range = range ?? "latest";
        }

        private const string NPM_ALIAS_SEPARATOR = "@npm:";

        /// <summary>
        /// Converts a <see cref="KeyValuePair"/> of two strings to a <see cref="PackageDependency" /> where the key is the package name and the value is the version range
        /// </summary>
        /// <param name="pair"><see cref="KeyValuePair"/> defining a package dependency where the key is the package name and the value is the version range</param>
        public static implicit operator PackageDependency(KeyValuePair<string, string?> pair)
        {
            var separator = pair.Key.IndexOf(NPM_ALIAS_SEPARATOR, StringComparison.Ordinal);
            if (separator < 0)
                return new PackageDependency(pair.Key, pair.Value);

            var alias = pair.Key.Substring(0, separator);
            var realName = pair.Key.Substring(separator + NPM_ALIAS_SEPARATOR.Length);
            return new PackageDependency(realName, pair.Value) { Alias = alias };
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
        /// Compares this dependency to another. The package name is compared case-insensitively, using
        /// ordinal rules for the reason documented on <see cref="PackageClosure"/>'s name comparison.
        /// <see cref="Range"/> is compared case-sensitively (semver pre-release identifiers are), and so
        /// is <see cref="Alias"/> - an alias is a label chosen locally by the manifest author, not a
        /// registry-wide package id.
        /// </summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns>Result of the comparison</returns>
        public override bool Equals(object? obj)
        {
            if (obj is not PackageDependency dependency) return false;

            return string.Equals(this.Name, dependency.Name, StringComparison.OrdinalIgnoreCase)
                && this.Range == dependency.Range
                && this.Alias == dependency.Alias;
        }

        /// <summary>
        /// Returns the hashcode of a dependency based on its name, range and alias. The name is hashed
        /// case-insensitively, to agree with <see cref="Equals(object?)"/>.
        /// </summary>
        /// <returns>the hashcode of a dependency based on its name, range and alias</returns>
        public override int GetHashCode()
        {
            var nameHash = Name is null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
            return (nameHash, Range, Alias).GetHashCode();
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
