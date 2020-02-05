using System.Collections.Generic;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{

    public interface IPackageCache : IPackageIndex
    {
        ValueTask<bool> Install(PackageReference package);
        ValueTask Install(PackageReference package, byte[] buffer);
        PackageManifest ReadManifest(PackageReference package);
        CanonicalIndex GetCanonicalIndex(PackageReference package);
        string GetFileContent(PackageReference package, string filename);
    }


   


}

