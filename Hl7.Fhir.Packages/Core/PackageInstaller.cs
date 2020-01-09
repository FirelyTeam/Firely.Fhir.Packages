using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hl7.Fhir.Packages
{

    public class PackageInstaller : IPackageInstaller
    {
        readonly PackageClient client;
        readonly IPackageCache cache;
        readonly Action<string> report;

        public PackageInstaller(PackageClient client, IPackageCache cache, Action<string> report)
        {
            this.client = client;
            this.cache = cache;
            this.report = report;
        }

        public async ValueTask<PackageReference> ResolveDependency(PackageDependency dependency)
        {
            var listing = await client.DownloadListingAsync(dependency.Name);
            if (listing == null) return PackageReference.None;
            var versions = listing.ToVersions();

            var version = versions.Resolve(dependency.Range)?.ToString(); //null => NotFound

            return new PackageReference(dependency.Name, version);
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


        private void Report(string message)
        {
            report?.Invoke(message);
        }

        private async ValueTask InstallDependencies(IEnumerable<PackageDependency> dependencies)
        {
            foreach (var dependency in dependencies)
            {
                var reference = await ResolveDependency(dependency);
                if (reference.Found)
                {
                    await InstallPackageAndDependencies(reference);
                }
            }
        }

        private async ValueTask InstallDependencies(PackageManifest manifest)
        {
            
            // Report($"Installing Dependencies for: {manifest.Name} {manifest.Version}: ");
            var dependencies = manifest.GetDependencies();
            await InstallDependencies(dependencies);

        }
        
        private async ValueTask InstallPackageAndDependencies(PackageReference reference)
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

        public async ValueTask<bool> IsInstalled(PackageDependency dependency)
        {
            return await cache.HasMatch(dependency);
        }
    }

   

}
