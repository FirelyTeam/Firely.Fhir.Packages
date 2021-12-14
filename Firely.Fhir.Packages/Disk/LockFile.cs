#nullable enable

using System;
using System.Collections.Generic;
using System.IO;

namespace Firely.Fhir.Packages
{
    public static class LockFile
    {
        public static PackageClosure? Read(string path)
        {
            if (File.Exists(path))
            {
                var content = File.ReadAllText(path);
                var dto = Parser.ReadLockFileJson(content);

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

        public static bool IsOutdated(string folder)
        {
            var man_path = Path.Combine(folder, PackageConsts.MANIFEST);
            var man_time = File.GetLastWriteTimeUtc(man_path);

            var asset_path = Path.Combine(folder, PackageConsts.LOCKFILE);
            var asset_time = File.GetLastWriteTimeUtc(asset_path);
            return asset_time < man_time;
        }

        public static PackageClosure? ReadFromFolder(string folder)
        {
            var path = Path.Combine(folder, PackageConsts.LOCKFILE);
            return Read(path);
        }

        public static PackageClosure? ReadFromFolderOrCreate(string folder)
        {
            var path = Path.Combine(folder, PackageConsts.LOCKFILE);
            return File.Exists(path) ? Read(path) : new PackageClosure();
        }

        public static void WriteToFolder(PackageClosure closure, string folder)
        {
            var dto = createLockFileJson(closure);
            var path = Path.Combine(folder, PackageConsts.LOCKFILE);
            write(dto, path);
        }

        private static void write(LockFileJson json, string path)
        {
            json.Updated = DateTime.Now;
            var content = Parser.WriteLockFileDto(json);
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