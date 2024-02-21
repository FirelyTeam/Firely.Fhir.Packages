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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public class DiskPackageCache : IPackageCache
    {
        public readonly string Root;

        public DiskPackageCache(string? root = null)
        {
            this.Root = root ?? Platform.GetFhirPackageRoot();
        }

        /// <summary>
        /// Check whether the package is already installed on disk
        /// </summary>
        /// <param name="reference">the package that is to be checked</param>
        /// <returns>whether a package is already installed</returns>
        public Task<bool> IsInstalled(PackageReference reference)
        {
            var target = PackageContentFolder(reference);
            return Task.FromResult(Directory.Exists(target));
        }

        /// <summary>
        /// Install a package on disk
        /// </summary>
        /// <param name="reference">Package reference of the package to be installed</param>
        /// <param name="buffer">File content of the package</param>
        public async Task Install(PackageReference reference, byte[] buffer)
        {
            var folder = packageRootFolder(reference);
            await Packaging.UnpackToFolder(buffer, folder);
            createIndexFile(reference);
        }

        /// <summary>
        /// Read the manifest file of a package
        /// </summary>
        /// <param name="reference">Package of which the manifest file is to be read</param>
        /// <returns>Package manifest</returns>
        public Task<PackageManifest?> ReadManifest(PackageReference reference)
        {
            var folder = PackageContentFolder(reference);

            return Directory.Exists(folder)
                ? Task.FromResult(ManifestFile.ReadFromFolder(folder))
                : Task.FromResult<PackageManifest?>(null);
        }

        /// <summary>
        /// Retrieve the index file that contains metadata of all files in the package
        /// </summary>
        /// <param name="reference">Package of which the index file is to be read</param>
        /// <returns>Index file</returns>
        public Task<CanonicalIndex> GetCanonicalIndex(PackageReference reference)
        {
            var rootFolder = packageRootFolder(reference);
            return Task.FromResult(CanonicalIndexFile.GetFromFolder(rootFolder, recurse: true));
        }

        /// <summary>
        /// Returns the folder path of a given package
        /// </summary>
        /// <param name="reference">Package of which the folder is to be returned</param>
        /// <returns>The package folder path</returns>
        public string PackageContentFolder(PackageReference reference)
        {
            var pkgfolder = packageFolderName(reference);
            var folder = Path.Combine(Root, pkgfolder, PackageFileNames.PACKAGEFOLDER);
            return folder;
        }

        private string packageRootFolder(PackageReference reference)
        {
            var pkgfolder = packageFolderName(reference);
            string target = Path.Combine(Root, pkgfolder);
            return target;
        }

        /// <summary>
        /// Returns all package references currently installed on disk
        /// </summary>
        /// <returns>all package references currently installed on disk</returns>
        public Task<IEnumerable<PackageReference>> GetPackageReferences()
        {
            var folders = getPackageRootFolders();
            var references = new List<PackageReference>(folders.Count());

            foreach (var folder in folders)
            {
                var name = Disk.GetFolderName(folder);

                if (isValidPackageFolder(name))
                {
                    var reference = parseFoldernameToReference(name);
                    references.Add(reference);
                }
            }

            return Task.FromResult(references.AsEnumerable());
        }

        //check whether the folder name has contains a single # sign which is not at the beginning or the end.
        string pattern = @"^[^#]+#[^#]+$";

        private bool isValidPackageFolder(string folderName) => Regex.IsMatch(folderName, pattern);


        private static PackageReference parseFoldernameToReference(string foldername)
        {
            var idx = foldername.IndexOf('#');

            return new PackageReference
            {
                Name = foldername.Substring(0, idx),
                Version = foldername.Substring(idx + 1)
            };
        }

        /// <summary>
        /// Returns the content of a specific file in the package
        /// </summary>
        /// <param name="reference">package that contains the file</param>
        /// <param name="filename">file name of the file that is to be read</param>
        /// <returns>File content represented as a string</returns>
        /// <exception cref="Exception">Throws an exception when a file is now found</exception>
        public Task<string> GetFileContent(PackageReference reference, string filename)
        {

            var folder = packageRootFolder(reference);
            string path = Path.Combine(folder, filename);

            string content;
            try
            {
                content = File.ReadAllText(path);
                return Task.FromResult(content);
            }
            catch
            {
                throw new Exception($"The file {filename} could not be found in package {reference}. You might have to do a restore.");
            }
        }

        /// <summary>
        /// Gets a list of versions of a specific package
        /// </summary>
        /// <param name="name">name of the package</param>
        /// <returns>list of versions</returns>
        public async Task<Versions?> GetVersions(string name)
        {
            var references = await GetPackageReferences();
            var vlist = references.Where(r => r.Name == name).Select(r => r.Version);

            if (vlist == null || !vlist.Any())
                return null;

            var versions = new Versions(vlist!);

            return versions;
        }

        [Obsolete("Not implemented yet")]
        public Task<byte[]> GetPackage(PackageReference reference)
        {
            throw new NotImplementedException();
        }

        private void createIndexFile(PackageReference reference)
        {
            var rootFolder = packageRootFolder(reference);
            CanonicalIndexFile.Create(rootFolder, recurse: true);
        }

        private static string packageFolderName(PackageReference reference, char glue = '#')
        {
            return reference.Name + glue + reference.Version;
        }

        private IEnumerable<string> getPackageRootFolders()
        {
            return Directory.Exists(Root) ? Directory.GetDirectories(Root) : Enumerable.Empty<string>();
        }
    }
}

#nullable restore
