#nullable enable

using Hl7.Fhir.Specification;
using Hl7.Fhir.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Firely.Fhir.Packages
{
    public static class ManifestFile
    {
        /// <summary>
        /// Reads and parses a <see cref="PackageManifest"/> at the given path.
        /// </summary>
        /// <param name="path">The full path to the file containing the manifest.</param>
        /// <returns></returns>
        private static PackageManifest? read(string path)
        {
            if (File.Exists(path))
            {
                var content = File.ReadAllText(path);
                return PackageParser.ReadManifest(content);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Reads and parses a <see cref="PackageManifest" /> from the package.json file in the given folder. 
        /// If missing, creates a new package.json file. />
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="fhirVersion"></param>
        /// <returns></returns>
        public static PackageManifest ReadOrCreate(string folder, string fhirVersion)
        {
            var manifest = ReadFromFolder(folder);
            if (manifest is null)
            {
                var name = CleanPackageName(Disk.GetFolderName(folder));
                manifest = Create(name, fhirVersion);
            }
            return manifest;
        }


        /// <summary>
        /// Serializes the manifest to json and writes it to the given path, optionally merging the changes with the contents already at that path. 
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="path">The full path to the file to write the manifest to.</param>
        /// <param name="merge">Whether to first merge the contents of the file at the given path before writing it.</param>
        /// <returns></returns>
        private static void write(PackageManifest manifest, string path, bool merge = false)
        {
            if (File.Exists(path) && merge)
            {
                var content = File.ReadAllText(path);
                var result = PackageParser.JsonMergeManifest(manifest, content);
                File.WriteAllText(path, result);
            }
            else
            {
                var content = PackageParser.WriteManifest(manifest);
                File.WriteAllText(path, content);
            }
        }


        /// <summary>
        /// Reads and parses a <see cref="PackageManifest"/> from a package.json file at a given folder.
        /// </summary>
        /// <param name="folder">The folder containing the package.json file.</param>
        /// <returns></returns>
        public static PackageManifest? ReadFromFolder(string folder)
        {
            var path = Path.Combine(folder, PackageFileNames.MANIFEST);
            var manifest = read(path);
            return manifest;
        }

        /// <summary>
        /// Creates a new <see cref="PackageManifest"/> initialized with sensible default values.
        /// </summary>
        /// <param name="name">A name for the package</param>
        /// <param name="fhirVersion">The FHIR version of the package contents.</param>
        /// <returns></returns>
        public static PackageManifest Create(string name, string fhirVersion)
        {
            var release = FhirReleaseParser.Parse(fhirVersion);
            //var release = FhirVersions.Parse(fhirVersion);
            var version = FhirReleaseParser.FhirVersionFromRelease(release);

            var manifest = new PackageManifest(name, "0.1.0")
            {
                Name = name,
                Description = "Put a description here",
                Version = "0.1.0",
                Dependencies = new Dictionary<string, string?>()
            };
            manifest.SetFhirVersion(version);
            return manifest;
        }

        ///// <summary>
        ///// Creates a new <see cref="PackageManifest"/> initialized with sensible default values.
        ///// </summary>
        ///// <param name="name">A name for the package</param>
        ///// <param name="fhirReleases">The FHIR version(s) of the package contents.</param>
        ///// <returns></returns>
        //public static PackageManifest Create(string name, FhirRelease[] fhirReleases)
        //{
        //    return new PackageManifest
        //    {
        //        Name = name,
        //        Description = "Put a description here",
        //        Version = "0.1.0",
        //        FhirVersions = new List<string>(fhirReleases.Select(r => FhirVersions.FhirVersionFromRelease(r))),
        //        Dependencies = new Dictionary<string, string>()
        //    };
        //}

        /// <summary>
        /// Creates a new <see cref="PackageManifest"/> initialized with sensible default values.
        /// </summary>
        /// <param name="name">A name for the package</param>
        /// <param name="fhirRelease">The FHIR version of the package contents.</param>
        /// <returns></returns>
        [Obsolete("With the introduction of release 4b, integer-numbered releases are no longer useable.")]
        internal static PackageManifest Create(string name, FhirRelease fhirRelease)
        {
            var fhirVersion = FhirReleaseParser.FhirVersionFromRelease(fhirRelease);
            return Create(name, fhirVersion);
        }

        /// <summary>
        /// Serializes the manifest to json and writes it to the package.json file in the given folder, 
        /// optionally merging the changes with the contents already in the package.json file. 
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="folder">The full path to the directory that contains the package.json file.</param>
        /// <param name="merge">Whether to first merge the contents of the file at the given path before writing it.</param>
        /// <returns></returns>
        public static void WriteToFolder(PackageManifest manifest, string folder, bool merge = false)
        {
            string path = Path.Combine(folder, PackageFileNames.MANIFEST);
            write(manifest, path, merge);
        }

        /// <summary>
        /// Checks whether a package name is valid  
        /// </summary>
        /// <param name="name">Package name to be checked</param>
        /// <returns>whether a package name is valid  </returns>
        public static bool ValidPackageName(string name)
        {
            char[] invalidchars = new char[] { '/', '\\' };
            int i = name.IndexOfAny(invalidchars);
            bool valid = i == -1;
            return valid;
        }

        /// <summary>
        /// Generates an acceptable package name from an chosen name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <remarks>Package names can only contain [A-Za-z], so this function will strip out any characters
        /// not within that range.</remarks>
        public static string CleanPackageName(string name)
        {
            var builder = new StringBuilder();
            foreach (char c in name)
            {
                if (c >= 'A' && c <= 'z') builder.Append(c);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Gets the package type of a certain package, based on the manifest
        /// </summary>
        /// <param name="manifest">The manifest file</param>
        /// <returns>The package type</returns>
        public static PackageManifestType? TryGetPackageType(this PackageManifest manifest)
        {
            if (manifest?.Type is null)
                return null;

            return PackageManifestTypes.TryParse(manifest.Type, out var type)
                ? type
                : null;
        }
    }
}

#nullable restore


