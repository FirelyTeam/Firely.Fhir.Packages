﻿/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


#nullable enable

using ICSharpCode.SharpZipLib.Tar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{

    public static partial class Packaging
    {
        private const string PACKAGE = "package";

        /// <summary>
        /// Extracts a manifest from a FHIR package file
        /// </summary>
        /// <param name="path">Path of the package file</param>
        /// <returns>The package manifest</returns>
        public static PackageManifest? ExtractManifestFromPackageFile(string path)
        {
            string file = Path.Combine(PACKAGE, PackageFileNames.MANIFEST);
            var entry = Tar.ExtractMatchingFiles(path, file).FirstOrDefault();

            return entry is null ? null : PackageParser.ParseManifest(entry.Buffer);
        }


        [CLSCompliant(false)]
        public static void WriteResource(this TarOutputStream tar, FileEntry entry)
        {
            entry.ChangeFolder(PACKAGE);
            Tar.Write(tar, entry);
        }

        /// <summary>
        /// Create a package file from a folder
        /// </summary>
        /// <param name="name">Name of the newly created package</param>
        /// <param name="folder">Path of the folder to be packaged</param>
        /// <returns>Path of the newly created package</returns>
        public static string PackFolder(string name, string folder)
        {
            var files = FileEntries
                .ReadAllFilesToPack(folder)
                .MakePathsRelative(folder)
                .Select(FileEntries.OrganizeToPackageStructure)
                .AddIndexFiles();

            return Tar.PackToDisk(name, files);
        }

        /// <summary>
        /// Create a package file from a folder
        /// </summary>
        /// <param name="name">Name of the newly created package</param>
        /// <param name="folder">Path of the folder to be packaged</param>
        /// <param name="organize">Add a custome folder organization structure for your package</param>
        /// <returns>Path of the newly created package</returns>
        public static string PackFolder(string name, string folder, Func<FileEntry, FileEntry> organize)
        {
            var files = FileEntries
                .ReadAllFilesToPack(folder)
                .MakePathsRelative(folder)
                .Select(organize)
                .AddIndexFiles();

            return Tar.PackToDisk(name, files);
        }

        private static byte[] toByteArray(this PackageManifest manifest)
        {
            var content = PackageParser.SerializeManifest(manifest);
            return Encoding.ASCII.GetBytes(content);
        }

        private static FileEntry toFileEntry(this PackageManifest manifest)
        {
            return new FileEntry(Path.Combine(PACKAGE, PackageFileNames.MANIFEST), manifest.toByteArray());
        }

        /// <summary>
        /// Create a package
        /// </summary>
        /// <param name="manifest">Package manifest</param>
        /// <param name="entries">Package files</param>
        /// <param name="moveToPackageFolder">Move all file entries to the "package" folder</param>
        /// <returns>The actual newly created FHIR package</returns>
        public static byte[] CreatePackage(PackageManifest manifest, IEnumerable<FileEntry> entries, bool moveToPackageFolder = true)
        {
            var entry = manifest.toFileEntry();

            if (moveToPackageFolder)
                entries = entries.ChangeFolder(PACKAGE);

            return Tar.Pack(entry, entries);
        }

        /// <summary>
        /// Create a package
        /// </summary>
        /// <param name="entries">Package files</param>
        /// <param name="moveToPackageFolder">Move all file entries to the "package" folder</param>
        /// <returns>The actual newly created FHIR package</returns>
        public static byte[] CreatePackage(IEnumerable<FileEntry> entries, bool moveToPackageFolder = true)
        {
            if (moveToPackageFolder)
                entries = entries.ChangeFolder(PACKAGE);

            return Tar.Pack(entries);
        }

        /// <summary>
        /// Unpack a FHIR package
        /// </summary>
        /// <param name="buffer">The fhir package file</param>
        /// <param name="folder">destination folder</param>
        /// <returns></returns>
        public static async Task UnpackToFolder(byte[] buffer, string folder)
        {
            await Task.Run(() =>
            {
                var tarball = Tar.Unzip(buffer);
                Tar.ExtractTarballToToDisk(tarball, folder);
            });
        }

        /// <summary>
        /// Returns a summary of the package files
        /// </summary>
        /// <param name="entries">package files</param>
        /// <returns>a package manifest, and a list of file names</returns>
        public static (PackageManifest? manifest, IEnumerable<string> files) GetPackageSummary(this IEnumerable<FileEntry> entries)
        {
            var files = new List<string>();
            var manifest = new PackageManifest("", ""); //TODO: discuss what to do here
            foreach (var entry in entries)
            {
                var filename = Path.GetFileName(entry.FilePath);
                if (filename == PackageFileNames.MANIFEST)
                {
                    manifest = PackageParser.ParseManifest(entry.Buffer);
                }
                else
                {
                    files.Add(entry.FilePath);
                }
            }

            return (manifest, files);
        }

        /// <summary>
        /// Extract a files from a specific path (folder) to a list of <see cref="FileEntry"/> objects.
        /// </summary>
        /// <param name="path">File location</param>
        /// <returns>a list of <see cref="FileEntry"/> objects.</returns>
        public static IEnumerable<FileEntry> ExtractFiles(string path)
        {
            return Tar.ExtractFiles(path, _ => true);
        }

        /// <summary>
        /// Extract a files from a stream to a list of <see cref="FileEntry"/> objects.
        /// </summary>
        /// <param name="stream">File stream</param>
        /// <returns>a list of <see cref="FileEntry"/> objects.</returns>
        public static IEnumerable<FileEntry> ExtractFiles(Stream stream)
        {
            return Tar.ExtractFiles(stream, _ => true);
        }
    }
}


#nullable restore