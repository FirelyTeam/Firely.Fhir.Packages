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

        public DiskPackageCache(string root)
        {
            this.Root = root;
        }

        public DiskPackageCache() 
        {
            this.Root = Platform.GetFhirPackageRoot();
        }

        public bool IsInstalled(PackageReference reference)
        {
            string target = PackageContentFolder(reference);
            return Directory.Exists(target);
        }

        public void Install(PackageReference reference, byte[] buffer)
        {
            var folder = PackageRootFolder(reference);
            Packaging.UnpackToFolder(buffer, folder);
            CreateIndexFile(reference);
        }


        public PackageManifest ReadManifest(PackageReference reference)
        {
            var folder = PackageContentFolder(reference);

            if (Directory.Exists(folder))
            {
                return ManifestFile.ReadFromFolder(folder);
            }
            else
            {
                return null;
            }
        }

        public CanonicalIndex GetCanonicalIndex(PackageReference reference)
        {
            var folder = PackageContentFolder(reference);
            return CanonicalIndexFile.GetFromFolder(folder);
        }


        public string PackageContentFolder(PackageReference reference)
        {
            // for backwards compatibility:
            {
                var pkgfolder = PackageFolderName(reference, '-');
                var folder = Path.Combine(Root, pkgfolder, DiskNames.PackageFolder);
                if (Directory.Exists(folder)) return folder;
            }

            // the new way:
            {
                var pkgfolder = PackageFolderName(reference, '#');
                var folder = Path.Combine(Root, pkgfolder, DiskNames.PackageFolder);
                return folder;
            }
        }

        private string PackageRootFolder(PackageReference reference)
        {
            var pkgfolder = PackageFolderName(reference);
            string target = Path.Combine(Root, pkgfolder);
            return target;
        }

        private void CreateIndexFile(PackageReference reference)
        {
            var folder = PackageContentFolder(reference);
            CanonicalIndexFile.Create(folder);
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

        private IEnumerable<string> GetPackageContentFolders()
        {
            return GetPackageRootFolders().Select(f => Path.Combine(f, DiskNames.PackageFolder));
        }

        private IEnumerable<string> GetPackageContentFolders(IEnumerable<PackageReference> references)
        {
            foreach(var refx in references)
            {
                var folder = PackageContentFolder(refx);
                if (Directory.Exists(folder))
                {
                    yield return folder;
                }
            }
        }

        //public IEnumerable<PackageManifest> GetManifests()
        //{
        //    foreach (var folder in GetPackageRootFolders())
        //    {
        //        var manifestpath = Path.Combine(folder, "package");
        //        var manifest = ManifestFile.ReadFromFolder(manifestpath);
        //        yield return manifest;
        //    }
        //}

        public IEnumerable<PackageReference> GetPackageReferences()
        {
            var folders = GetPackageRootFolders();
            foreach(var folder in folders)
            {
                var entry = Disk.GetFolderName(folder);
                var idx = entry.IndexOfAny(new[] { '-', '#' }); // backwards compatibility: also support '-'
                yield return new PackageReference { Name = entry.Substring(0, idx), Version = entry.Substring(idx + 1) };
            }
        }

        public string GetFileContent(PackageReference reference, string filename)
        {
            var folder = PackageContentFolder(reference);
            string path = Path.Combine(folder, filename);
            return File.ReadAllText(path);
        }
    }


}

