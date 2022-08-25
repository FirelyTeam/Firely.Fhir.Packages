using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public static class PackageServerExtensions
    {
        public static async ValueTask<Versions> GetVersions(this IPackageServer server, PackageDependency dependency)
        {
            return await server.GetVersions(dependency.Name);
        }

        public static async ValueTask<PackageReference> Resolve(this IPackageServer server, PackageDependency dependency, bool stable = false)
        {
            var versions = await server.GetVersions(dependency.Name);
            var version = versions.Resolve(dependency.Range, stable)?.ToString(); //null => NotFound

            if (version is null) return PackageReference.None;

            return new PackageReference(dependency.Name, version);
        }

        public static async ValueTask<PackageReference> ResolveListed(this IPackageServer server, PackageDependency dependency, bool stable = false)
        {
            var versions = await server.GetVersions(dependency.Name);
            var version = versions.Resolve(dependency.Range, stable: true)?.ToString(); //null => NotFound

            if (version is null) return PackageReference.None;

            return new PackageReference(dependency.Name, version);
        }

        public static async ValueTask<PackageReference> GetLatest(this IPackageServer server, string name, bool stable = false)
        {
            var versions = await server.GetVersions(name);
            var version = versions.Latest(stable)?.ToString();
            return new PackageReference(name, version);
        }

        public async static ValueTask<bool> HasMatch(this IPackageServer server, PackageDependency dependency, bool stable = false)
        {
            var reference = await server.Resolve(dependency, stable);
            return await Task.FromResult(reference.Found);
        }
    }
}

