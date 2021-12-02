﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public Task<bool> IsInstalled(PackageReference reference)
        {
            string target = PackageContentFolder(reference);
            return Task.FromResult(Directory.Exists(target));
        }

        public async Task Install(PackageReference reference, byte[] buffer)
        {
            var folder = PackageRootFolder(reference);
            await Packaging.UnpackToFolder(buffer, folder);
            CreateIndexFile(reference);
        }

        public Task<PackageManifest?> ReadManifest(PackageReference reference)
        {
            var folder = PackageContentFolder(reference);

            if (Directory.Exists(folder))
            {
                return Task.FromResult(ManifestFile.ReadFromFolder(folder));
            }
            else
            {
                return null;
            }
        }

        public Task<CanonicalIndex> GetCanonicalIndex(PackageReference reference)
        {
            var folder = PackageContentFolder(reference);
            return Task.FromResult(CanonicalIndexFile.GetFromFolder(folder, recurse: false));
        }

        public string PackageContentFolder(PackageReference reference)
        {
            //// for backwards compatibility:
            //{
            //    var pkgfolder = PackageFolderName(reference, '-');
            //    var folder = Path.Combine(Root, pkgfolder, PackageConsts.PackageFolder);
            //    if (Directory.Exists(folder)) return folder;
            //}

            // the new way:
            {
                var pkgfolder = PackageFolderName(reference);
                var folder = Path.Combine(Root, pkgfolder, PackageConsts.PackageFolder);
                return folder;
            }
        }

        private string PackageRootFolder(PackageReference reference)
        {
            var pkgfolder = PackageFolderName(reference);
            string target = Path.Combine(Root, pkgfolder);
            return target;
        }

        public Task<IEnumerable<PackageReference>> GetPackageReferences()
        {
            var folders = GetPackageRootFolders();
            var references = new List<PackageReference>(folders.Count());

            foreach (var folder in folders)
            {
                var name = Disk.GetFolderName(folder);
                
                var reference = ParseFoldernameToReference(name);
                references.Add(reference);
            }

            return Task.FromResult(references.AsEnumerable());
        }

        public static PackageReference ParseFoldernameToReference(string foldername)
        {
            var idx = foldername.IndexOf('#'); 

            return new PackageReference
            {
                Name = foldername.Substring(0, idx),
                Version = foldername.Substring(idx + 1)
            };
        }

        public Task<string> GetFileContent(PackageReference reference, string filename)
        {

            var folder = PackageContentFolder(reference);
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

        public async Task<Versions> GetVersions(string name)
        {
            var references = await GetPackageReferences();
            var vlist = references.Where(r => r.Name == name).Select(r => r.Version);
            var versions = new Versions(vlist);

            return versions;
        }

        [Obsolete("Not implemented yet")]
        public Task<byte[]> GetPackage(PackageReference reference)
        {
            throw new NotImplementedException();
        }

        private void CreateIndexFile(PackageReference reference)
        {
            var folder = PackageContentFolder(reference);
            CanonicalIndexFile.Create(folder, recurse: false);
        }

        private static string PackageFolderName(PackageReference reference, char glue = '#')
        {
            return reference.Name + glue + reference.Version;
        }

        private IEnumerable<string> GetPackageRootFolders()
        {
            if (Directory.Exists(Root))
            {
                return Directory.GetDirectories(Root);
            }
            else
            {
                return Enumerable.Empty<string>();
            }
        }

      
    }

    
}

