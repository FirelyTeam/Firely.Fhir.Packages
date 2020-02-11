using System.Collections.Generic;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public interface IPackageCache : IPackageServer
    {
        bool IsInstalled(PackageReference reference);
        public IEnumerable<PackageReference> GetPackageReferences();

        Task Install(PackageReference reference, byte[] buffer);
        PackageManifest ReadManifest(PackageReference reference);
        CanonicalIndex GetCanonicalIndex(PackageReference reference);
        string GetFileContent(PackageReference reference, string filename);
    }
}

