
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Hl7.Fhir.Packages
{

    public class PackageListing
    {
        [JsonProperty(PropertyName = "_id")]
        public string Id;

        [JsonProperty(PropertyName = "name")]
        public string Name;

        [JsonProperty(PropertyName = "description")]
        public string Description;

        [JsonProperty(PropertyName = "dist-tags")]
        public Dictionary<string, string> DistTags;

        [JsonProperty(PropertyName = "versions")]
        public Dictionary<string, PackageRelease> Versions;
    }

    public class PackageRelease
    {
        [JsonProperty(PropertyName = "name")]
        public string Name;

        [JsonProperty(PropertyName = "version")]
        public string Version;
         
        [JsonProperty(PropertyName = "description")]
        public string Description;

        [JsonProperty(PropertyName = "author")]
        public string Author;

        [JsonProperty(PropertyName = "url")]
        public string Url;

        [JsonProperty(PropertyName = "deprecated")]
        public string Deprecated;
    }

    public class PackageManifest
    {
        [JsonProperty(PropertyName = "name")]
        public string Name;

        [JsonProperty(PropertyName = "version")]
        public string Version;

        [JsonProperty(PropertyName = "description")]
        public string Description;

        [JsonProperty(PropertyName = "author")]
        public string Author;

        [JsonProperty(PropertyName = "dependencies")]
        public Dictionary<string, string> Dependencies;

        [JsonProperty(PropertyName = "fhirVersions")]
        public List<string> FhirVersions;

        [JsonProperty(PropertyName = "devDependencies")]
        public Dictionary<string, string> DevDependencies;

        [JsonProperty(PropertyName = "canonicals")]
        public Dictionary<string, string> Canonicals;
    }

    public class LockFileDto
    {
        [JsonProperty(PropertyName = "updated")]
        public DateTime Updated;

        [JsonProperty(PropertyName = "dependencies")]
        public Dictionary<string, string> PackageReferences;

        [JsonProperty(PropertyName = "missing")]
        public Dictionary<string, string> MissingDependencies;
    }

    public class CanonicalIndex
    {
        public DateTimeOffset date;

        [JsonProperty(PropertyName = "canonicals")]
        public Dictionary<string, string> Canonicals;
    }

    public class PackageCatalogEntry
    {
        public string Name;
        public string Description;
        public string FhirVersion;
    }


    public static class PackageModelExtensions
    {
        public static PackageReference GetPackageReference(this PackageManifest manifest)
        {
            var reference = new PackageReference(manifest.Name, manifest.Version);
            return reference;
        }

        public static IEnumerable<PackageReference> GetPackageReferences(this LockFileDto dto)
        {
            foreach (var item in dto.PackageReferences) yield return item; // implicit conversion
        }

        public static void AddDependency(this PackageManifest manifest, string name, string version)
        {
            if (manifest.Dependencies is null) manifest.Dependencies = new Dictionary<string, string>();
            if (!manifest.Dependencies.ContainsKey(name))
            {
                manifest.Dependencies.Add(name, version);
            }
            else
            {
                manifest.Dependencies[name] = version;
            }
        }

        public static void AddDependency(this PackageManifest manifest, PackageManifest dependency)
        {
            manifest.AddDependency(dependency.Name, dependency.Version);
        }

        public static bool HasDependency(this PackageManifest manifest, string pkgname)
        {
            foreach (var key in manifest.Dependencies.Keys)
            {
                if (string.Compare(key, pkgname, ignoreCase: true) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool RemoveDependency(this PackageManifest manifest, string pkgname)
        {
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
    }
}
