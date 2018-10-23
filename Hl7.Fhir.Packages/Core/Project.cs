using System;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.Fhir.Packages
{
    public class Project
    {
        string folder;
        PackageCache cache;
        PackageContext context;
        PackageClient client;
        PackageInstaller installer;
        PackageRestorer restorer;

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

        public void Install(string package, string version = null)
        {
            // todo: make it async
            var manifest = ReadManifest();
            var pkgmanifest = installer.InstallPackage(package, version).Result;
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
               
    }
}
