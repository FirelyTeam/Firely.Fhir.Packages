using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public static class PackageServerExtensions
    {
        public static async ValueTask<Versions> GetVersionsAsync(this IPackageServer server, PackageDependency dependency)
        {
            return await server.GetVersionsAsync(dependency.Name);
        }

        public static async ValueTask<PackageReference> ResolveAsync(this IPackageServer server, PackageDependency dependency)
        {
            var versions = await server.GetVersionsAsync(dependency.Name);
            var version = versions.Resolve(dependency.Range)?.ToString(); //null => NotFound
            if (version is null) return PackageReference.None;

            return new PackageReference(dependency.Name, version);
        }

        public static async ValueTask<PackageReference> GetLatestAsync(this IPackageServer server, string name)
        {
            var versions = await server.GetVersionsAsync(name);
            var version = versions.Latest()?.ToString();
            return new PackageReference(name, version);
        }

        public async static ValueTask<bool> HasMatchAsync(this IPackageServer server, PackageDependency dependency)
        {
            var reference = await server.ResolveAsync(dependency);
            return await Task.FromResult(reference.Found);
        }
    }
}

