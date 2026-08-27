/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


#nullable enable

using System;
using System.Collections.Generic;

namespace Firely.Fhir.Packages
{
    /// <summary>
    /// A package reference is a reference to a very specific version of a package. When you want to define a a range of versions that may quality, like 3.x, use PackageDependency
    /// A PackageReference is used in a Scope or Closure while a PackageDependency is used in a Manifest.
    /// </summary>
    public struct PackageReference
    {
        public string? Scope;

        private string? _name;

        /// <summary>
        /// The package name (always normalized to lowercase by the constructor). Null means an empty reference.
        /// </summary>
        public string? Name
        {
            readonly get => _name;
            [Obsolete("Setting Name directly bypasses lowercase normalization and can produce an invalid, inconsistent package reference. Construct a new PackageReference using the constructor instead, which normalizes the name.")]
            set => _name = value;
        }

        public string? Version;

        /// <summary>
        /// Optional local alias, from an npm-style <c>alias@npm:name</c> dependency. When set, this reference
        /// is an explicit request for this specific version regardless of conflict-resolution strategy.
        /// <see cref="Name"/> always holds the real package name.
        /// </summary>
        public string? Alias;

        /// <summary>
        /// Provide the name and optionally the version of the package. 
        /// </summary>
        /// <param name="name">The package name may include the (exact) version if separated with an at @ sign.</param>
        /// <param name="version">Optionally the exact version of the package</param>
        public PackageReference(string name, string? version) : this(null, name, version)
        { }

        /// <summary>
        /// Provide the name and optionally the version of the package. 
        /// </summary>
        /// <param name="scope">An optional package scope</param>
        /// <param name="name">The package name may include the (exact) version if separated with an at @ sign.</param>
        /// <param name="version">Optionally the exact version of the package</param>
        /// <param name="alias">Optional npm-style local alias (see <see cref="Alias"/>)</param>
        public PackageReference(string? scope, string name, string? version, string? alias = null)
        {
            // Scope as well as Name: an npm scope is part of the package id, and it is emitted
            // into registry URLs by GetNpmName and NodePackageUrlProvider.
            this.Scope = scope?.ToLowerInvariant();
            // Lowercased for the same reason as PackageDependency.Name - see that constructor.
            // Assigned directly to the backing field, bypassing the (obsolete) Name setter.
            _name = name?.ToLowerInvariant();
            this.Version = version;
            this.Alias = alias;
        }

        public string Moniker => $"{Name}@{Version}";

        /// <summary>
        /// Returns the package name and version devided by an at @ sign respresented as a single string
        /// </summary>
        /// <returns>Returns the package name and version devided by an at @ sign respresented as a single string</returns>
        public override string ToString()
        {
            string s = $"{Name}@{Version}";
            if (!Found) s += " (NOT FOUND)";
            return s;
        }

        /// <summary>
        /// Empty package reference
        /// </summary>
        public static PackageReference None => new();

        /// <summary>
        /// Returns true if the package isn't found
        /// </summary>
        public bool NotFound => !Found;

        /// <summary>
        /// Returns true is the package is found
        /// </summary>
        public bool Found => !(Name is null || Version is null);

        /// <summary>
        /// Implicitly converts a <see cref="KeyValuePair"/> to a <see cref="PackageReference"/>, where the key is the package name, and the value is the version
        /// </summary>
        /// <param name="kvp"><see cref="KeyValuePair"/> where the key is the package name, and the value is the version</param>
        public static implicit operator PackageReference(KeyValuePair<string, string?> kvp)
        {
            return new PackageReference(kvp.Key, kvp.Value);
        }

        /// <summary>
        /// Implicitly converts a <see cref="KeyValuePair"/> to a <see cref="PackageReference"/>, where the key is the package name, and the value is the version
        /// </summary>
        /// <param name="kvp"><see cref="KeyValuePair"/> where the key is the package name, and the value is the version</param>
        public static implicit operator PackageReference(string reference)
        {
            return Parse(reference);
        }

        /// <summary>
        /// Compare two package references by scope, name and version. Scope and name are compared
        /// case-insensitively.
        /// </summary>
        /// <param name="A">First package reference</param>
        /// <param name="B">Second package reference</param>
        /// <returns>Result of the comparison</returns>
        public static bool operator ==(PackageReference A, PackageReference B)
        {
            return samePart(A.Scope, B.Scope)
                && samePart(A.Name, B.Name)
                && A.Version == B.Version;
        }

        /// <summary>
        /// Compares one part of a package id. Package names and scopes are case-insensitive, and are
        /// compared using ordinal rules for the reason documented on <see cref="PackageClosure"/>'s name
        /// comparison: culture-aware casing would consider "fhir" and "FHIR" different names under
        /// Turkish/Azeri dotless-i rules. Versions are deliberately not compared this way - semver
        /// pre-release identifiers are case-sensitive.
        /// </summary>
        private static bool samePart(string? left, string? right)
            => string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

        private static int partHash(string? part)
            => part is null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(part);

        /// <summary>
        /// Compare two package references by scope, name and version. Scope and name are compared
        /// case-insensitively.
        /// </summary>
        /// <param name="A">First package reference</param>
        /// <param name="B">Second package reference</param>
        /// <returns>Result of the comparison</returns>
        public static bool operator !=(PackageReference A, PackageReference B)
        {
            return !(A == B);
        }

        /// <summary>
        /// Returns the name and version of a package
        /// </summary>
        /// <param name="name">Package name</param>
        /// <param name="version">Package version</param>
        public void Deconstruct(out string? name, out string? version)
        {
            name = Name;
            version = Version;
        }

        /// <summary>
        /// Compare the current package references to another object
        /// </summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns>Result of the comparison</returns>
        public override bool Equals(object? obj)
        {
            if (obj is not PackageReference)
            {
                return false;
            }

            var reference = (PackageReference)obj;
            return this == reference;
        }

        /// <summary>
        /// Returns the hashcode of a package based on the scope, name and version. Scope and name are
        /// hashed case-insensitively, to agree with <see cref="Equals(object?)"/>.
        /// </summary>
        /// <returns>the hashcode of a package based on the scope, name and version</returns>
        public override int GetHashCode()
        {
            return (partHash(Scope), partHash(Name), Version).GetHashCode();
        }

        /// <summary>
        /// Create a package reference out of a string
        /// </summary>
        /// <param name="reference">String of the following format: "@scope/name@version"</param>
        /// <returns>PackageReference object</returns>
        public static PackageReference Parse(string reference)
        {
            var (scope, name, version) = parseReference(reference);
            return new PackageReference(scope, name, version);
        }

        private static (string? scope, string name, string? version) parseReference(string reference)
        {
            string? scope = null;
            string? version = null;

            if (reference.StartsWith("@")) // scope: @scope/name@version
            {
                var segments = reference.Split('/');
                scope = segments[0].Substring(1);
                reference = segments[1];
            }

            var parts = reference.Split("@"); // name@version
            string name = parts[0];
            if (parts.Length > 1) version = parts[1];
            return (scope, name, version);

        }
    }
}

#nullable restore
