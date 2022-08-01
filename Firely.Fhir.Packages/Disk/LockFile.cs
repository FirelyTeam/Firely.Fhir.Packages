/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


#nullable enable

using System;
using System.Collections.Generic;
using System.IO;

namespace Firely.Fhir.Packages
{
    public static class LockFile
    {
        private static PackageClosure? read(string path)
        {
            if (File.Exists(path))
            {
                var content = File.ReadAllText(path);
                var dto = PackageParser.ParseLockFileJson(content);

                return dto is null
                    ? null
                    : new PackageClosure
                    {
                        References = dto.PackageReferences?.ToPackageReferences() ?? new List<PackageReference>(),
                        Missing = dto.MissingDependencies?.ToPackageDependencies() ?? new List<PackageDependency>(),
                    };

            }
            else return null;
        }

        /// <summary>
        /// Check if the lock file of a folder is outdated
        /// </summary>
        /// <param name="folder">The folder path</param>
        /// <returns>whether the lock file is outdated</returns>
        public static bool IsOutdated(string folder)
        {
            var man_path = Path.Combine(folder, PackageFileNames.MANIFEST);
            var man_time = File.GetLastWriteTimeUtc(man_path);

            var asset_path = Path.Combine(folder, PackageFileNames.LOCKFILE);
            var asset_time = File.GetLastWriteTimeUtc(asset_path);
            return asset_time < man_time;
        }

        /// <summary>
        /// Reads lock file from folder
        /// </summary>
        /// <param name="folder">The folder path</param>
        /// <returns>The lock file</returns>
        public static PackageClosure? ReadFromFolder(string folder)
        {
            var path = Path.Combine(folder, PackageFileNames.LOCKFILE);
            return read(path);
        }

        /// <summary>
        /// Reads or creates the lock file of a folder      
        /// </summary>
        /// <param name="folder">the folder path</param>
        /// <returns>The lock file</returns>
        public static PackageClosure? ReadFromFolderOrCreate(string folder)
        {
            var path = Path.Combine(folder, PackageFileNames.LOCKFILE);
            return File.Exists(path) ? read(path) : new PackageClosure();
        }

        /// <summary>
        /// Writes a lock file to a folder
        /// </summary>
        /// <param name="closure">The lock file</param>
        /// <param name="folder">The folder path</param>
        public static void WriteToFolder(PackageClosure closure, string folder)
        {
            var dto = createLockFileJson(closure);
            var path = Path.Combine(folder, PackageFileNames.LOCKFILE);
            write(dto, path);
        }

        private static void write(LockFileJson json, string path)
        {
            json.Updated = DateTime.Now;
            var content = PackageParser.SerializeLockFileDto(json);
            File.WriteAllText(path, content);
        }

        private static LockFileJson createLockFileJson(PackageClosure closure)
        {
            return new LockFileJson
            {
                PackageReferences = closure.References.ToDictionary(),
                MissingDependencies = closure.Missing.ToDictionary()
            };
        }
    }
}


#nullable restore