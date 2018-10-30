using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hl7.Fhir.Packages
{

    /// <summary>
    /// A class to open up all functionality need to manage packages in a project.
    /// </summary>
    public class Project
    {
        readonly string folder;
        public PackageCache Cache { get; }
        public PackageIndex Index { get; }
        public PackageClient Client { get; }
        public PackageInstaller Installer { get; }
        public PackageRestorer Restorer { get; }

        public Project(string folder)
        {
            this.folder = folder;

            Cache = PackageFactory.GlobalPackageCache();
            Index = new PackageIndex(Cache, folder);
            Client = PackageClient.Create();
            Installer = new PackageInstaller(Client, Cache, null);
            Restorer = new PackageRestorer(Client, Installer);
        }

        public PackageManifest ReadManifest()
        {
            var manifest = ManifestFile.ReadFromFolder(folder);
            return manifest;
        }

        public void SaveManifest(PackageManifest manifest)
        {
            ManifestFile.WriteToFolder(manifest, folder, merge: true);
        }

        public void Restore()
        {
            var manifest = ReadManifest();
            Restorer.Restore(manifest).Wait();
        }

        public void Init(string pkgname = null, string version = null)
        {
            var manifest = ReadManifest();
            if (manifest != null)
            {
                throw new Exception($"A Package manifests already exists in this folder.");
            }

            if (pkgname is null)
                pkgname = ManifestFile.CleanPackageName(Disk.GetFolderName(folder));

            if (!ManifestFile.ValidPackageName(pkgname))
            {
                throw new Exception($"Invalid package name {pkgname}");
            }

            manifest = ManifestFile.Create(pkgname);
            ManifestFile.WriteToFolder(manifest, folder);
        }
           
        public async ValueTask Install(string package, string version = null)
        {
            var manifest = ReadManifest();
            var pkgmanifest = await Installer.InstallPackage(package, version);

            manifest.AddDependency(pkgmanifest);
            SaveManifest(manifest);
        }

        public void Remove(string package)
        {
            var manifest = ManifestFile.ReadOrCreate(folder);
            if (manifest.Dependencies.Keys.Contains(package))
            {
                manifest.Dependencies.Remove(package);
                ManifestFile.WriteToFolder(manifest, folder, merge: true);
            }
            else
            {
                throw new Exception($"The package '{package}' was not installed.");
            }
        }

        public CanonicalFileReference Resolve(string canonical)
        { 
            if (Index.TryFindReference(canonical, out CanonicalFileReference reference))
            {
                return reference;
            }
            else
            {
                return null;
            }
                
        }

        public async ValueTask<IList<string>> GetPackagesWithCanonical(string canonical)
        {
            return await Client.FindCanonical(canonical);
        }
       
    }


    public static class ProjectExtensions
    {
        public static IEnumerable<string> PackageContenFolders(this Project project)
        {
            return project.Index.GetPackageContentFolders();
        }
    }

}
