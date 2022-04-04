/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


using System.Collections.Generic;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    /// <summary>
    /// Only used to get access to the project I/O, this is not about scope
    /// </summary>
    public interface IProject
    {
        /// <summary>
        /// Reads and parses a <see cref="PackageManifest"/> from the <see cref="Folder"/>.
        /// </summary>
        /// <returns>the package manifest</returns>
        Task<PackageManifest?> ReadManifest();

        /// <summary>
        /// Writes a package manifest file
        /// </summary>
        /// <param name="manifest"></param>
        /// <returns></returns>
        Task WriteManifest(PackageManifest manifest);

        /// <summary>
        /// Reads and parses a <see cref="PackageClosure"/>
        /// </summary>
        /// <returns>A package closure</returns>
        Task<PackageClosure?> ReadClosure();

        /// <summary>
        /// Writes a package closure
        /// </summary>
        /// <param name="closure">Package closure</param>
        /// <returns></returns>
        Task WriteClosure(PackageClosure closure);


        /// <summary>
        /// Reads the raw contents of the given file.
        /// </summary>
        /// <param name="filename">the name of the file to be read</param>
        /// <returns>the file content represented as a string</returns>
        public Task<string> GetFileContent(string filename);

        /// <summary>
        /// Indexes all files of the project.
        /// </summary>
        /// <returns>A list of metadata of all files</returns>
        public Task<List<ResourceMetadata>> GetIndex();
    }
}
