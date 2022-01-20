using System.Collections.Generic;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public interface IPackageCache : IPackageServer
    {
        /// <summary>
        /// Check whether the package is already installed
        /// </summary>
        /// <param name="reference">the package that is to be checked</param>
        /// <returns>whether a package is already installed</returns>
        Task<bool> IsInstalled(PackageReference reference);

        /// <summary>
        /// Returns all package references currently installed
        /// </summary>
        /// <returns>all package references currently installed</returns>
        public Task<IEnumerable<PackageReference>> GetPackageReferences();

        /// <summary>
        /// Install a package
        /// </summary>
        /// <param name="reference">Package reference of the package to be installed</param>
        /// <param name="buffer">File content of the package</param>
        Task Install(PackageReference reference, byte[] buffer);

        /// <summary>
        /// Read the manifest file of a package
        /// </summary>
        /// <param name="reference">Package of which the manifest file is to be read</param>
        /// <returns>Package manifest</returns>
        Task<PackageManifest?> ReadManifest(PackageReference reference);

        /// <summary>
        /// Retrieve the index file that contains metadata of all files in the package
        /// </summary>
        /// <param name="reference">Package of which the index file is to be read</param>
        /// <returns>Index file</returns>
        Task<CanonicalIndex> GetCanonicalIndex(PackageReference reference);

        /// <summary>
        /// Returns the content of a specific file in the package
        /// </summary>
        /// <param name="reference">package that contains the file</param>
        /// <param name="filename">file name of the file that is to be read</param>
        /// <returns>File content represented as a string</returns>
        Task<string> GetFileContent(PackageReference reference, string filename);
    }
}

