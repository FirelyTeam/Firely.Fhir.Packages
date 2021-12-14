#nullable enable

using System;
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
            var target = PackageContentFolder(reference);
            return Task.FromResult(Directory.Exists(target));
        }

        public async Task Install(PackageReference reference, byte[] buffer)
        {
            var folder = packageRootFolder(reference);
            await Packaging.UnpackToFolder(buffer, folder);
            createIndexFile(reference);
        }

        public Task<PackageManifest?> ReadManifest(PackageReference reference)
        {
            var folder = PackageContentFolder(reference);

            return Directory.Exists(folder)
                ? Task.FromResult(ManifestFile.ReadFromFolder(folder))
                : Task.FromResult<PackageManifest?>(null);
        }

        public Task<CanonicalIndex> GetCanonicalIndex(PackageReference reference)
        {
            var rootFolder = packageRootFolder(reference);
            return Task.FromResult(CanonicalIndexFile.GetFromFolder(rootFolder, recurse: true));
        }

        public string PackageContentFolder(PackageReference reference)
        {
            var pkgfolder = packageFolderName(reference);
            var folder = Path.Combine(Root, pkgfolder, PackageConsts.PACKAGEFOLDER);
            return folder;
        }

        private string packageRootFolder(PackageReference reference)
        {
            var pkgfolder = packageFolderName(reference);
            string target = Path.Combine(Root, pkgfolder);
            return target;
        }

        public Task<IEnumerable<PackageReference>> GetPackageReferences()
        {
            var folders = getPackageRootFolders();
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
