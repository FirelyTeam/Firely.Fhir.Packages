using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            IndexReferences(reference);
        }

        public void IndexReferences(PackageReference reference)
        {
            var folder = PackageContentFolder(reference);
            var index = CanonicalIndexFile.GetIndexFromFolderContents(folder);
            var references = new CanonicalReferences { Canonicals = index };
            CanonicalIndexFile.WriteToFolder(references, folder);
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

        public CanonicalReferences ReadCanonicalReferences(PackageReference reference)
        {
            var folder = PackageContentFolder(reference);
            return CanonicalIndexFile.ReadFromFolder(folder);
        }

        public string PackageRootFolder(PackageReference reference)
        {
            var pkgfolder = PackageFolderName(reference);
            string target = Path.Combine(Root, pkgfolder);
            return target;
        }

        public string PackageContentFolder(PackageReference reference)
        {
            var pkgfolder = PackageFolderName(reference);
            var folder = Path.Combine(Root, pkgfolder, DiskNames.PackageFolder);
            return folder;
        }

        public static string PackageFolderName(PackageReference reference)
        {
            return reference.Name + "-" + reference.Version;
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
                var idx = entry.IndexOf('-');
                yield return new PackageReference { Name = entry.Substring(0, idx), Version = entry.Substring(idx + 1) };
            }
        }
    }


    public static class PackageCacheExtensions
    {
        public static PackageManifest ReadManifest(this PackageCache cache, string name, string version)
        {
            var reference = new PackageReference(name, version);
            return cache.ReadManifest(reference);
        }

        public static CanonicalReferences ReadCanonicalReferences(this PackageCache cache, string name, string version)
        {
            var reference = new PackageReference(name, version);
            return cache.ReadCanonicalReferences(reference);
        }

        public static IEnumerable<PackageReference> WithName(this IEnumerable<PackageReference> refs, string name)
        {
            return refs.Where(r => string.Compare(r.Name, name, ignoreCase: true) == 0);
        }

        public static IEnumerable<PackageReference> GetInstalledVersions(this PackageCache cache, string pkgname)
        {
            return cache.GetPackageReferences().WithName(pkgname);
        }

        public static PackageManifest InstallFromFile(this PackageCache cache, string path)
        {
            var manifest = Packaging.ExtractManifestFromPackageFile(path);
            var reference = manifest.GetPackageReference();
            var buffer = File.ReadAllBytes(path);
            cache.Install(reference, buffer);
            return manifest;
        }
    }


}

