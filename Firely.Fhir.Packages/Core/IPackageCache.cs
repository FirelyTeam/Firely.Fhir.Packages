using System.Collections.Generic;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public interface IPackageCache : IPackageServer
    {
        Task<bool> IsInstalledAsync(PackageReference reference);
        public Task<IEnumerable<PackageReference>> GetPackageReferencesAsync();

        Task InstallAsync(PackageReference reference, byte[] buffer);
        Task<PackageManifest> ReadManifestAsync(PackageReference reference);
        Task<CanonicalIndex> GetCanonicalIndexAsync(PackageReference reference);
        Task<string> GetFileContentAsync(PackageReference reference, string filename);
    }
}

