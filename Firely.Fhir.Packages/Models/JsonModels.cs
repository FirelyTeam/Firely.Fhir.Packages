/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


#nullable enable

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Firely.Fhir.Packages
{
    /// <summary>
    /// A FHIR server publishes a package and it's available versions through a package listing
    /// This class provides a JSON (de)serializable package listing..
    /// </summary>
    public class PackageListing
    {
        [JsonProperty(PropertyName = "_id")]
        public string? Id;

        [JsonProperty(PropertyName = "name")]
        public string? Name;

        [JsonProperty(PropertyName = "description")]
        public string? Description;

        [JsonProperty(PropertyName = "dist-tags")]
        public Dictionary<string, string>? DistTags;

        [JsonProperty(PropertyName = "versions")]
        public Dictionary<string, PackageRelease>? Versions;
    }

    /// <summary>
    /// A JSON formatted package listing contains package information for each version
    /// </summary>
    public class PackageRelease
    {
        /// <summary>
        /// Name of the package
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string? Name;

        /// <summary>
        /// Version of the package
        /// </summary>
        [JsonProperty(PropertyName = "version")]
        public string? Version;

        /// <summary>
        /// Description of the package
        /// </summary>
        [JsonProperty(PropertyName = "description")]
        public string? Description;

        /// <summary>
        /// The dist sub document conforms to NPM 7, providing a shasum and tarball 
        /// </summary>
        [JsonProperty(PropertyName = "dist")]
        public Dist? Dist;

        // Removed the property, because in NPM-6 it's a string and in NPM-7 it's a subclass.
        // The horror!
        //[JsonProperty(PropertyName = "author")]
        //public string? Author;

        /// <summary>
        /// FHIR version 
        /// </summary>
        [JsonProperty(PropertyName = "fhirVersion")]
        public string? FhirVersion;

        /// <summary>
        /// The URL where a package can be downloaded
        /// </summary>
        [JsonProperty(PropertyName = "url")]
        public string? Url;

        /// <summary>
        /// If a package is unlisted, it should no longer be used except for
        /// backward compatible installations.
        /// This field is defined by us, it's not part of npm. npm has a deprecated warning. The "unlisted" field is currently a string, but we expect to transform it to a boolean "true" / "false".
        /// </summary>
        [JsonProperty(PropertyName = "unlisted")]
        public string? Unlisted;
    }


    public class Dist
    {
        /// <summary>
        /// NPM 7.0 will not install packages without this checksum
        /// </summary>
        [JsonProperty(PropertyName = "shasum")]
        public string? Shasum;

        [JsonProperty(PropertyName = "tarball")]
        public string? Tarball;
    }

    /// <summary>
    /// The class JSON (de)serializable representation of a package.json 
    /// </summary>
    public class PackageManifest
    {
        /// <summary>
        /// Initialize a new packagemanifest
        /// </summary>
        /// <param name="name">Package name </param>
        /// <param name="version">Version of the package</param>
        public PackageManifest(string name, string version)
        {
            Name = name;
            Version = version;
        }

        /// <summary>
        /// The globally unique name for the package.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name;

        /// <summary>
        /// Semver-based version for the package
        /// </summary>
        [JsonProperty(PropertyName = "version")]
        public string Version;

        /// <summary>
        /// Description of the package.
        /// </summary>
        [JsonProperty(PropertyName = "description")]
        public string? Description;

        /// <summary>
        /// Author of the package.
        /// </summary>
        [JsonProperty(PropertyName = "author")]
        public string? Author;

        /// <summary>
        /// Other packages that the contents of this packages depend on.
        /// </summary>
        [JsonProperty(PropertyName = "dependencies")]
        public Dictionary<string, string?>? Dependencies;

        /// <summary>
        /// Other packages necessary during development of this package.
        /// </summary>
        [JsonProperty(PropertyName = "devDependencies")]
        public Dictionary<string, string>? DevDependencies;

        /// <summary>
        /// List of keywords to help with discovery.
        /// </summary>
        [JsonProperty(PropertyName = "keywords")]
        public List<string>? Keywords;

        /// <summary>
        /// List of keywords to help with discovery.
        /// </summary>
        [JsonProperty(PropertyName = "license")]
        public string? License;

        /// <summary>
        /// The url to the project homepage.
        /// </summary>
        [JsonProperty(PropertyName = "homepage")]
        public string? Homepage;

        /// <summary>
        /// Describes the structure of the package.
        /// </summary>
        /// <remarks>Some of the common keys used are defined in <see cref="DirectoryKeys"/>.</remarks>
        [JsonProperty(PropertyName = "directories")]
        public Dictionary<string, string>? Directories;

        /// <summary>
        /// String-based keys used in the <see cref="Directories"/> dictionary.
        /// </summary>
        public class DirectoryKeys
        {
            /// <summary>
            /// Where the bulk of the library is.
            /// </summary>
            public const string DIRECTORY_KEY_LIB = "lib";
            public const string DIRECTORY_KEY_BIN = "bin";
            public const string DIRECTORY_KEY_MAN = "man";
            public const string DIRECTORY_KEY_DOC = "doc";
            public const string DIRECTORY_KEY_EXAMPLE = "example";
            public const string DIRECTORY_KEY_TEST = "test";
        }

        /// <summary>
        /// Title for the package.
        /// </summary>
        [JsonProperty(PropertyName = "title")]
        public string? Title;

        /// <summary>
        /// Versions of the FHIR standard used in artifacts within this package.
        /// </summary>
        /// <remarks>Largely obsolete, and replaced by actual dependencies on the
        /// core packages.</remarks>
        [JsonProperty(PropertyName = "fhirVersions")]
        public List<string>? FhirVersions;

        /// <summary>
        /// Versions of the FHIR standard used in artifacts within this package.
        /// </summary>
        /// <remarks>It seems this is mistakenly generated in the core packages
        /// published by HL7 and should be the same as <see cref="FhirVersions"/> above.</remarks>
        [JsonProperty(PropertyName = "fhir-version-list")]
        public List<string>? FhirVersionList;

        /// <summary>
        /// An optional value to indicate the type of package generated by the IG build tool
        /// </summary>
        /// <remarks>
        /// It's fairly random, so please do depend on it. Also note that the HL7 IG build tool
        /// creates template and tool packages and publishes them as a FHIR package
        /// even though they have little to do with FHIR packages.
        /// </remarks>
        [JsonProperty(PropertyName = "type")]
        public string? Type;

        public class Maintainer
        {
            [JsonProperty(PropertyName = "name")]
            public string? Name;

            [JsonProperty(PropertyName = "email")]
            public string? Email;
        }

        /// <summary>
        /// List of individual(s) responsible for maintaining the package.
        /// </summary>
        [JsonProperty(PropertyName = "maintainers")]
        public List<Maintainer>? Maintainers;

        /// <summary>
        /// For IG packages: The canonical url of the IG (equivalent to ImplementationGuide.url).
        /// </summary>
        [JsonProperty(PropertyName = "canonical")]
        public string? Canonical;

        /// <summary>
        /// For IG packages: Where the human readable representation (e.g. IG) is published on the web.
        /// </summary>
        [JsonProperty(PropertyName = "url")]
        public string? Url;

        /// <summary>
        /// Country code for the jurisdiction under which this package is published.
        /// </summary>
        /// <remarks>Formatted as an urn specifying the code system and code, e.g. "urn:iso:std:iso:3166#US".</remarks>
        [JsonProperty(PropertyName = "jurisdiction")]
        public string? Jurisdiction;
    }

    /// <summary>
    /// Representation of a package lock file
    /// This file is FHIR / Firely specific
    /// </summary>
    public class LockFileJson
    {
        /// <summary>
        /// Last updated
        /// </summary>
        [JsonProperty(PropertyName = "updated")]
        public DateTime Updated;

        /// <summary>
        /// Package Dependencies
        /// </summary>
        [JsonProperty(PropertyName = "dependencies")]
        public Dictionary<string, string?>? PackageReferences;

        /// <summary>
        /// Dependencies that are missing
        /// </summary>
        [JsonProperty(PropertyName = "missing")]
        public Dictionary<string, string?>? MissingDependencies;
    }




    /// <summary>
    /// IndexJson contains a JSON (de)serializable representation of index.JSON. Which
    /// is generated for every package in a package cache, listing all resources and their metadata
    /// Index.json, should stay conformant to the spec here: https://confluence.hl7.org/display/FHIR/NPM+Package+Specification#NPMPackageSpecification-.index.json
    /// </summary>
    public class IndexJson
    {
        /// <summary>
        /// Date/Time the index was created
        /// </summary>
        public DateTimeOffset date;

        /// <summary>
        /// Version of the index.json specification this file adheres to
        /// </summary>
        [JsonProperty(PropertyName = "index-version")]
        public int Version;

        /// <summary>
        /// List of metadata of the files
        /// </summary>
        [JsonProperty(PropertyName = "files")]
        public List<IndexData>? Files; // canonical -> file
    }

    /// <summary>
    /// entry of Index.json, should stay conformant to the spec here: https://confluence.hl7.org/display/FHIR/NPM+Package+Specification#NPMPackageSpecification-.index.json
    /// </summary>
    public class IndexData
    {
        /// <summary>
        /// Initialization of an entry of index.json
        /// </summary>
        /// <param name="filename">name of the file</param>
        public IndexData(string filename)
        {
            FileName = filename;
        }

        /// <summary>
        /// name of the file
        /// </summary>
        [JsonProperty("filename", Required = Required.Always)]
        public string FileName;

        /// <summary>
        /// FHIR resource type
        /// </summary>
        [JsonProperty("resourceType")]
        public string? ResourceType;

        /// <summary>
        /// FHIR resource id
        /// </summary>
        [JsonProperty("id")]
        public string? Id;

        /// <summary>
        /// FHIR canonical url (only of conformance resources)
        /// </summary>
        [JsonProperty("url")]
        public string? Canonical;

        /// <summary>
        /// Version of the resource (only of conformance resources)
        /// </summary>
        [JsonProperty("version")]
        public string? Version;

        /// <summary>
        /// the value of a the "kind" property in the resource, if it has one and it's a primitive
        /// </summary>
        [JsonProperty("kind")]
        public string? Kind;

        /// <summary>
        /// the value of a the "type" property in the resource, if it has one and it's a primitive
        /// </summary>
        [JsonProperty("type")]
        public string? Type;

        /// <summary>
        /// Copy this instance
        /// </summary>
        /// <param name="other"><see cref="IndexData"/> instance the properties are copied to </param>
        public void CopyTo(IndexData other)
        {
            other.FileName = FileName;
            other.ResourceType = ResourceType;
            other.Id = Id;
            other.Canonical = Canonical;
            other.Version = Version;
            other.Kind = Kind;
            other.Type = Type;
        }
    }

    /// <summary>
    /// Firely specific additions to index.json entries to end up in the .firely.index.json file at the root of the package after installation.
    /// </summary>
    public class CanonicalIndex
    {
        /// <summary>
        /// Date/time the index file was created
        /// </summary>
        public DateTimeOffset date;

        /// <summary>
        /// Version of the .firely.index.json file format this instance adheres to.
        /// </summary>
        [JsonProperty(PropertyName = "index-version")]
        public int Version;

        /// <summary>
        /// List of metadata of the files
        /// </summary>
        [JsonProperty(PropertyName = "files")]
        public List<ResourceMetadata>? Files; // canonical -> file
    }

    /// <summary>
    /// Firely specific additions to index.json entries to end up in the .firely.index.json file at the root of the package after installation.
    /// </summary>
    public class ResourceMetadata : IndexData
    {
        /// <summary>
        /// Instantiates a new metadata entry for .firely.index.json
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="filepath"></param>
        public ResourceMetadata(string filename, string filepath) : base(filename)
        {
            FileName = filename;
            FilePath = filepath;
        }

        /// <summary>
        /// Relative filepath of a file to the package root.
        /// </summary>
        [JsonProperty("filepath", Required = Required.Always)]
        public string FilePath;

        /// <summary>
        /// The FHIR version of a file
        /// </summary>
        [JsonProperty("fhirVersion")]
        public string? FhirVersion;

        /// <summary>
        /// True, if the file is a StructureDefinition resource, which has a snapshot.
        /// False if the file is a StructureDefinition resource and doesn't contain a snapshot.
        /// Null if the file isn't a StructureDefinition
        /// </summary>
        [JsonProperty("hasSnapshot")]
        public bool? HasSnapshot;

        /// <summary>
        /// True, if the file is an expanded ValueSet resource
        /// False, if the file is non-expanded ValueSet resource
        /// Null, if the file isn't a snapshot
        /// </summary>
        [JsonProperty("hasExpansion")]
        public bool? HasExpansion;

        /// <summary>
        /// The CodeSystem canonical url of which this valueset is an direct representation.
        /// This property is null if this file isn't a ValueSet resource, or a ValueSet resource that doesn't describe a full single CodeSystem.
        /// </summary>
        [JsonProperty("valuesetCodeSystem")]
        public string? ValueSetCodeSystem;

        /// <summary>
        /// Source and Target URL of a ConceptMap resource
        /// Null of this file isn't a ConceptMap resource
        /// </summary>
        [JsonProperty("conceptMapUris")]
        public SourceAndTarget? ConceptMapUris;

        /// <summary>
        /// List of defined UniqueIds in this NamingSystem resource
        /// Null if this file isn't a NamingSystem resource.
        /// </summary>
        [JsonProperty("namingSystemUniqueId")]
        public string[]? NamingSystemUniqueId;

        /// <summary>
        /// Copy this instance
        /// </summary>
        /// <param name="other"><see cref="ResourceMetadata"/> instance the properties are copied to </param>
        public void CopyTo(ResourceMetadata other)
        {
            other.FileName = FileName;
            other.FilePath = FilePath;
            other.ResourceType = ResourceType;
            other.Id = Id;
            other.Canonical = Canonical;
            other.Version = Version;
            other.Kind = Kind;
            other.Type = Type;
            other.FhirVersion = FhirVersion;
            other.HasExpansion = HasExpansion;
            other.HasSnapshot = HasSnapshot;
            other.ValueSetCodeSystem = ValueSetCodeSystem;
            other.ConceptMapUris = ConceptMapUris;
            other.NamingSystemUniqueId = NamingSystemUniqueId;
        }
    }

    /// <summary>
    /// Entry of a package catalog
    /// </summary>
    public class PackageCatalogEntry
    {
        /// <summary>
        /// Package name
        /// </summary>
        public string? Name;

        /// <summary>
        /// Package Description
        /// </summary>
        public string? Description;

        /// <summary>
        /// FHIR version
        /// </summary>
        public string? FhirVersion;
    }

    /// <summary>
    /// Source and target of a ConceptMap resource
    /// </summary>
    public class SourceAndTarget
    {
        /// <summary>
        /// Target Uri of a ConceptMap resource
        /// </summary>
        [JsonProperty("targetUri")]
        public string? TargetUri;

        /// <summary>
        /// Source Uri of a ConceptMap resource
        /// </summary>
        [JsonProperty("sourceUri")]
        public string? SourceUri;
    }


    public static class JsonModelExtensions
    {
        /// <summary>
        /// Generates a <see cref="PackageReference"/> object from this package manifest
        /// </summary>
        /// <param name="manifest"></param>
        /// <returns>The <see cref="PackageReference"/> object that is generated from a package manifest </returns>
        public static PackageReference GetPackageReference(this PackageManifest manifest)
        {
            var reference = new PackageReference(manifest.Name, manifest.Version);
            return reference;
        }

        /// <summary>
        /// Generates a <see cref="PackageReference"/> object from this lock file
        /// </summary>
        /// <param name="manifest"></param>
        /// <returns>The <see cref="PackageReference"/> object that is generated from a lock file </returns>
        public static IEnumerable<PackageReference> GetPackageReferences(this LockFileJson dto)
        {
            return dto.PackageReferences == null
                ? Enumerable.Empty<PackageReference>()
                : dto.PackageReferences.Select(i => (PackageReference)i);
        }

        internal static void AddDependency(this PackageManifest manifest, string name, string? version)
        {
            if (version is null) version = "latest";
            if (manifest.Dependencies is null) manifest.Dependencies = new Dictionary<string, string?>();
            if (!manifest.Dependencies.ContainsKey(name))
            {
                manifest.Dependencies.Add(name, version);
            }
            else
            {
                manifest.Dependencies[name] = version;
            }
        }

        /// <summary>
        /// Adds a package dependendy to this manifest
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="dependency">Package dependency</param>
        public static void AddDependency(this PackageManifest manifest, PackageDependency dependency)
        {
            manifest.AddDependency(dependency.Name, dependency.Range);
        }

        /// <summary>
        /// Add a specific dependency
        /// </summary>
        /// <param name="manifest">The manifest of the package the dependency is to be added to</param>
        /// <param name="pkgname">The name of the dependency to be added</param>
        /// <returns>Whether the dependency has been added</returns>
        public static void AddDependency(this PackageManifest manifest, PackageManifest dependency)
        {
            manifest.AddDependency(dependency.Name, dependency.Version);
        }

        /// <summary>
        /// Check whether a package has a specific dependency
        /// </summary>
        /// <param name="manifest">The manifest of the package to be checked</param>
        /// <param name="pkgname">The name of the dependency to be checked for</param>
        /// <returns>Whether a package has a specific dependency</returns>
        public static bool HasDependency(this PackageManifest manifest, string pkgname)
        {
            if (manifest?.Dependencies?.Keys is null)
                return false;

            foreach (var key in manifest.Dependencies.Keys)
            {
                if (string.Compare(key, pkgname, ignoreCase: true) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Remove a specific dependency
        /// </summary>
        /// <param name="manifest">The manifest of the package the dependency is to be removed from</param>
        /// <param name="pkgname">The name of the dependency to be removed</param>
        /// <returns>Whether the dependency has been removed</returns>
        public static bool RemoveDependency(this PackageManifest manifest, string pkgname)
        {
            if (manifest?.Dependencies?.Keys is null)
                return false;

            foreach (var key in manifest.Dependencies.Keys)
            {
                if (string.Compare(key, pkgname, ignoreCase: true) == 0)
                {
                    manifest.Dependencies.Remove(key);
                    return true;
                }
            }
            return false;

        }

        /// <summary>
        /// Get the FHIR version from a package manifest
        /// </summary>
        /// <param name="manifest"></param>
        /// <returns>The FHIR version declared in the package manifest</returns>
        public static string? GetFhirVersion(this PackageManifest manifest)
        {
            string? version =
                manifest.FhirVersions?.FirstOrDefault()
                ?? manifest.FhirVersionList?.FirstOrDefault();

            return version;
        }

        internal static void SetFhirVersion(this PackageManifest manifest, string version)
        {
            manifest.FhirVersions = new List<string> { version };
        }
    }
}

#nullable restore