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
    /// A package reference is a reference to a very specific version of a package. When you want to define a a range of versions that may quality, like 3.x, use PackageDependency
    /// A PackageReference is used in a Scope or Closure while a PackageDependency is used in a Manifest.
    /// </summary>
    public struct PackageReference
    {
        public string? Scope;
        public string? Name; // null means empty reference
        public string? Version;

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
        public PackageReference(string? scope, string name, string? version)
        {
            this.Scope = scope;
            this.Name = name;
            this.Version = version;
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
        public static PackageReference None => new() { Name = null, Version = null };

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
            return parse(reference);
        }

        /// <summary>
        /// Compare two package references by name and version
        /// </summary>
        /// <param name="A">First package reference</param>
        /// <param name="B">Second package reference</param>
        /// <returns>Result of the comparison</returns>
        public static bool operator ==(PackageReference A, PackageReference B)
        {
            return (A.Name == B.Name && A.Version == B.Version);
        }

        /// <summary>
        /// Compare two package references by name and version
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
            return this.Name == reference.Name &&
                   this.Version == reference.Version;
        }

        /// <summary>
        /// Returns the hashcode of a package based on the name and version
        /// </summary>
        /// <returns>the hashcode of a package based on the name and version</returns>
        public override int GetHashCode()
        {
            return (Name, Version).GetHashCode();
        }

        private static PackageReference parse(string reference)
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
