using System.Threading.Tasks;

namespace Hl7.Fhir.Packages
{
    public interface IPackageInstaller
    {
        ValueTask<PackageReference> ResolveDependency(PackageDependency reference);
        ValueTask<PackageManifest> InstallPackage(PackageReference reference);
    }

   

}
