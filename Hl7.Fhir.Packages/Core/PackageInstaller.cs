using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Hl7.Fhir.Packages
{
    public class PackageInstaller 
    {
        readonly PackageClient client;
        readonly PackageCache cache;
        readonly Action<string> report;

        public PackageInstaller(PackageClient client, PackageCache cache, Action<string> report)
        {
            this.client = client;
            this.cache = cache;
            this.report = report;
        }

        private void Report(string message)
        {
            report?.Invoke(message);
        }

        public async ValueTask InstallDependencies(IEnumerable<PackageReference> dependencies)
        {
            foreach (var dependency in dependencies)
            {
                var reference = await ResolveReference(dependency);
                if (reference.Found)
                {
                    await InstallPackageAndDependencies(reference);
                }
            }
        }

        public async ValueTask InstallDependencies(PackageManifest manifest)
        {
            
            // Report($"Installing Dependencies for: {manifest.Name} {manifest.Version}: ");
            var dependencies = manifest.GetDependencies();
            await InstallDependencies(dependencies);

        }

        public async ValueTask<PackageManifest> InstallPackage(PackageReference reference) 
        {
            bool installed = cache.IsInstalled(reference);

            if (!installed)
            {
                var buffer = await client.DownloadPackage(reference);
                cache.Install(reference, buffer);

                Report($"Installed package: {reference.Name} {reference.Version} ");
            }
            var manifest = cache.ReadManifest(reference);

            return manifest;
        }
        
        public async ValueTask InstallPackageAndDependencies(PackageReference reference)
        {
            var manifest = await InstallPackage(reference);
            if (manifest != null)
            {
                await InstallDependencies(manifest);
            }
            else
            {
                Report($"Error: could not find manifest for: {reference.Name}-{reference.Version}. Skipped installing dependencies.");
            }
        }

        public async ValueTask<PackageManifest> InstallPackage(string pkgname, string pkgversion)
        {
            var reference = await ResolveReference(pkgname, pkgversion);
            if (reference.Found)
            {
                var manifest = await InstallPackage(reference);
                return manifest;
            }
            else
            {
                return null;
            }
        }

        public async ValueTask<PackageReference> ResolveReference(PackageReference reference)
        {
            var listing = await client.DownloadListingAsync(reference);
            if (listing == null) return PackageReference.NotFound;
            var versions = listing.ToVersions();

            reference.Version = versions.Resolve(reference.Version)?.ToString(); //null => NotFound

            return reference;
        }

        public async ValueTask<PackageReference> ResolveReference(string pkgname, string pattern)
        {
            //this could be faster by caching --mh
            var reference = new PackageReference(pkgname, pattern);
            return await ResolveReference(reference);
            
        }

        public PackageManifest InstallFromFile(string path)
        {
            var manifest = Packaging.ExtractManifestFromPackageFile(path);
            var reference = manifest.GetPackageReference();
            var buffer = File.ReadAllBytes(path);
            cache.Install(reference, buffer);
            return manifest;
        }

    }

}
