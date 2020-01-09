using System.IO;
using System;

namespace Firely.Fhir.Packages
{
    public static class LockFile
    { 
        public static Closure Read(string path)
        {
            if (File.Exists(path))
            {
                var content = File.ReadAllText(path);
                var dto = Parser.ReadLockFileJson(content);
                return new Closure
                {
                    References = dto.PackageReferences.ToPackageReferences(),
                    Missing = dto.MissingDependencies.ToPackageDependencies()
                };

            }
            else return null;
        }

        public static void Write(LockFileDto json, string path)
        {
            json.Updated = DateTime.Now;
            var content = Parser.WriteLockFileDto(json);
            File.WriteAllText(path, content);
        }

        public static bool IsOutdated(string folder)
        {
            var man_path = Path.Combine(folder, DiskNames.Manifest);
            var man_time = File.GetLastWriteTimeUtc(man_path);

            var asset_path = Path.Combine(folder, DiskNames.PackageLockFile);
            var asset_time = File.GetLastWriteTimeUtc(asset_path);
            return asset_time < man_time;
        }

        public static Closure ReadFromFolder(string folder)
        {
            var path = Path.Combine(folder, DiskNames.PackageLockFile);
            return Read(path);
        }

        public static Closure ReadFromFolderOrCreate(string folder)
        {
            var path = Path.Combine(folder, DiskNames.PackageLockFile);
            if (File.Exists(path))
            {
                return Read(path);
            }
            else
            {
                return new Closure();
            }
        }

        public static void WriteToFolder(Closure closure, string folder)
        {
            var dto = closure.CreateLockFileDto();
            var path = Path.Combine(folder, DiskNames.PackageLockFile);
            Write(dto, path);
        }
    }
}


