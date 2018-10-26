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
        readonly PackageCache cache;
        internal readonly PackageContext context;
        readonly PackageClient client;
        readonly PackageInstaller installer;
        readonly PackageRestorer restorer;

        public Project(string folder)
        {
            this.folder = folder;
            cache = PackageFactory.GlobalPackageCache();
            
            context = new PackageContext(cache, folder);
            client = PackageClient.Create();
            installer = new PackageInstaller(client, cache, null);
            restorer = new PackageRestorer(client, installer);
        }

        public PackageManifest ReadManifest()
        {
            var manifest = Disk.ReadFolderManifest(folder);
            return manifest;
        }

        public void SaveManifest(PackageManifest manifest)
        {
            Disk.WriteFolderManifest(manifest, folder, merge: true);
        }

        public void Restore()
        {
            var manifest = ReadManifest();
            restorer.Restore(manifest).Wait();
        }

        public void Init(string pkgname = null, string version = null)
        {
            var manifest = ReadManifest();
            if (manifest != null)
            {
                throw new Exception($"A Package manifests already exists in this folder.");
            }

            if (pkgname is null)
                pkgname = Disk.CleanPackageName(Disk.GetFolderName(folder));

            if (!Disk.ValidPackageName(pkgname))
            {
                throw new Exception($"Invalid package name {pkgname}");
            }

            manifest = Disk.CreateManifest(pkgname);
            Disk.WriteFolderManifest(manifest, folder);
        }
           
        public async ValueTask Install(string package, string version = null)
        {
            var manifest = ReadManifest();
            var pkgmanifest = await installer.InstallPackage(package, version);
            manifest.AddDependency(pkgmanifest);
            SaveManifest(manifest);
        }

        public void Remove(string package)
        {
            var manifest = Disk.ReadOrCreateFolderManifest(folder);
            if (manifest.Dependencies.Keys.Contains(package))
            {
                manifest.Dependencies.Remove(package);
                Disk.WriteFolderManifest(manifest, folder, merge: true);
            }
            else
            {
                throw new Exception($"The package '{package}' was not installed.");
            }
        }
               
        public Reference Resolve(string canonical)
        { 
            if (context.TryFindReference(canonical, out Reference reference))
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
            return await client.FindCanonical(canonical);
        }
       
    }

}
