/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


#nullable enable

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public class FolderProject : IProject
    {
        public string Folder { get; private set; }

        /// <summary>
        /// Represents a folder on disk containing FHIR artifacts
        /// </summary>
        /// <param name="folder"></param>
        public FolderProject(string folder)
        {
            this.Folder = folder;
        }

        /// <summary>
        /// Indexes all files in the folder, including subfolders.
        /// </summary>
        /// <returns>A list of metadata of all files</returns>
        public Task<List<ResourceMetadata>> GetIndex()
        {
            // this should be cached, but we need to bust it on changes.
            return Task.FromResult(CanonicalIndexer.IndexFolder(Folder, recurse: true));
        }

        /// <summary>
        /// Reads the raw contents of the given file.
        /// </summary>
        /// <param name="filePath">The relative filePath compared to the given <see cref="Folder"/>.</param>
        /// <returns>the file content represented as a string</returns>
        public Task<string> GetFileContent(string filePath)
        {
            var path = Path.Combine(Folder, filePath);
            return Task.FromResult(File.ReadAllText(path));
        }

        /// <summary>
        /// Reads and parses a <see cref="PackageClosure"/> from the <see cref="Folder"/>.
        /// </summary>
        /// <returns>A package closure</returns>
        public Task<PackageClosure?> ReadClosure()
        {
            var closure = LockFile.ReadFromFolder(Folder);
            return Task.FromResult(closure);
        }

        /// <summary>
        /// Reads and parses a <see cref="PackageManifest"/> from the <see cref="Folder"/>.
        /// </summary>
        /// <returns>the package manifest</returns>
        public Task<PackageManifest?> ReadManifest()
        {
            var manifest = ManifestFile.ReadFromFolder(Folder);
            return Task.FromResult(manifest); ;
        }

        /// <summary>
        /// Writes a package closure to the folder
        /// </summary>
        /// <param name="closure">Package closure</param>
        /// <returns></returns>
        public Task WriteClosure(PackageClosure closure)
        {
            LockFile.WriteToFolder(closure, Folder);

            return Task.FromResult(0); //because in net45 there is no Task.CompletedTask (Paul)
        }

        /// <summary>
        /// Writes a package manifest to a folder
        /// </summary>
        /// <param name="manifest"></param>
        /// <returns></returns>
        public Task WriteManifest(PackageManifest manifest)
        {
            ManifestFile.WriteToFolder(manifest, Folder, merge: true);

            return Task.FromResult(0); //because in net45 there is no Task.CompletedTask (Paul)
        }
    }
}

#nullable restore