using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public interface IPackageServer
    {
        /// <summary>
        /// Retrieve the different versions of a package
        /// </summary>
        /// <param name="name">Package name</param>
        /// <returns>List is versions</returns>
        Task<Versions?> GetVersions(string name);

        /// <summary>
        /// Download a package from the source
        /// </summary>
        /// <param name="reference">Package reference of the package to be downloaded </param>
        /// <returns>Package content as byte array</returns>
        Task<byte[]> GetPackage(PackageReference reference);
    }
}
