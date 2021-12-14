#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public static class PackageClientExtensions
    {
        public static async ValueTask<string?> DownloadListingRawAsync(this PackageClient client, PackageReference reference)
        {
            return reference.Version != null && reference.Version.StartsWith("git")
                ? throw new NotImplementedException("We cannot yet resolve git references")
                : await client.DownloadListingRawAsync(reference.Name).ConfigureAwait(false);
        }


        public static async ValueTask<IList<string>> FindPackageByName(this PackageClient client, string partial)
        {
            // backwards compatibility
            var result = await client.CatalogPackagesAsync(pkgname: partial).ConfigureAwait(false);
            return result.Where(c => c.Name is not null).Select(c => c.Name!).ToList();
        }

        public static async ValueTask<IList<string>> FindPackagesByCanonical(this PackageClient client, string canonical)
        {
            // backwards compatibility
            var result = await client.CatalogPackagesAsync(canonical: canonical).ConfigureAwait(false);
            return result.Where(c => c.Name is not null).Select(c => c.Name!).ToList();
        }

    }
}
#nullable restore