using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Hl7.Fhir.Packages
{
    public class PackageCache
    {
        public readonly string Root;

        public PackageCache(string root)
        {
            this.Root = root;
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

        public void CreateIndexFile(PackageReference reference)
        {
            var folder = PackageContentFolder(reference);
            CanonicalIndexFile.CreateIndexFile(folder);
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
            return CanonicalIndexFile.GetIndexFromFolder(folder);
        }

        public string PackageRootFolder(PackageReference reference)
        {
            var pkgfolder = PackageFolderName(reference);
            string target = Path.Combine(Root, pkgfolder);
            return target;
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

        public static string PackageFolderName(PackageReference reference, char glue = '#')
        {
            return reference.Name + glue + reference.Version;
        }

        public IEnumerable<string> GetPackageRootFolders()
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

        public IEnumerable<string> GetPackageContentFolders()
        {
            return GetPackageRootFolders().Select(f => Path.Combine(f, DiskNames.PackageFolder));
        }

        public IEnumerable<string> GetPackageContentFolders(IEnumerable<PackageReference> references)
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
    }


}

