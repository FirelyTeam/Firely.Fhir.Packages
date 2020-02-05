using System.Collections.Generic;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    /// <summary>
    /// An offline package server?
    /// </summary>
    public interface IPackageIndex
    {
        bool IsInstalled(PackageReference reference);
        ValueTask<Versions> GetVersionsAsync(string name);
        public IEnumerable<PackageReference> GetPackageReferences();
        
    }

    public static class PackageIndexExtensions
    {
        public static async ValueTask<Versions> GetVersionsAsync(this IPackageIndex server, PackageDependency dependency)
        {
            return await server.GetVersionsAsync(dependency.Name);
        }

        public static async ValueTask<PackageReference> Resolve(this IPackageIndex server, PackageDependency dependency)
        {
            var versions = await server.GetVersionsAsync(dependency.Name);
            var version = versions.Resolve(dependency.Range)?.ToString(); //null => NotFound
            if (version is null) return PackageReference.None;

            return new PackageReference(dependency.Name, version);
        }

        

        public static async ValueTask<PackageReference> GetLatest(this IPackageIndex server, string name)
        {
            var versions = await server.GetVersionsAsync(name);
            var version = versions.Latest()?.ToString();
            return new PackageReference(name, version);
        }
    }



}

