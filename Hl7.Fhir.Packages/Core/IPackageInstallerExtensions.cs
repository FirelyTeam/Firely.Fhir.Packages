using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public static class IPackageInstallerExtensions
    {
        public static async ValueTask<PackageReference> ResolveDependency(this IPackageInstaller installer, string pkgname, string range)
        {
            //this could be faster by caching --mh
            var dependency = new PackageDependency(pkgname, range);
            return await installer.ResolveDependency(dependency);

        }

        public static async ValueTask<PackageManifest> InstallPackage(this IPackageInstaller installer, string pkgname, string pkgversion)
        {
             var reference = await installer.ResolveDependency(pkgname, pkgversion);
            if (reference.Found)
            {
                var manifest = await installer.InstallPackage(reference);
                return manifest;
            }
            else
            {
                return null;
            }
        }
    }

   

}
