using System.Collections.Generic;

namespace Hl7.Fhir.Packages
{
    public interface IPackageCache
    {
        bool IsInstalled(PackageReference reference);
        void Install(PackageReference reference, byte[] buffer);
        PackageManifest ReadManifest(PackageReference reference);
        CanonicalIndex GetCanonicalIndex(PackageReference reference);
        public IEnumerable<PackageReference> GetPackageReferences();
        string GetFileContent(PackageReference reference, string filename);
    }


}

