using System.IO;
using System.Collections.Generic;
using Hl7.FhirPath;
using System;
using System.Text;

namespace Hl7.Fhir.Packages
{

    public static class ManifestFile
    {
        public static PackageManifest Read(string path)
        {
            if (File.Exists(path))
            {
                var content = File.ReadAllText(path);
                return Parser.ReadManifest(content);
            }
            else
            {
                return null;
            }
        }

        public static PackageManifest ReadOrCreate(string folder)
        {
            var manifest = ReadFromFolder(folder);
            if (manifest is null)
            {
                var name = CleanPackageName(Disk.GetFolderName(folder));
                manifest = Create(name);
            }
            return manifest;
        }

        public static void Write(PackageManifest manifest, string path, bool merge = false)
        {
            if (File.Exists(path) && merge)
            {
                var content = File.ReadAllText(path);
                var result = Parser.JsonMergeManifest(manifest, content);
                File.WriteAllText(path, result);
            }
            else
            {
                var content = Parser.WriteManifest(manifest);
                File.WriteAllText(path, content);
            }
        }

        public static PackageManifest ReadFromFolder(string folder)
        {
            var path = Path.Combine(folder, DiskNames.Manifest);
            return Read(path);
        }

        public static PackageManifest Create(string name)
        {
            return new PackageManifest
            {
                Name = name,
                Description = "Put a description here",
                Version = "0.1.0",
                Dependencies = new Dictionary<string, string>()
            };
        }

        public static void WriteToFolder(PackageManifest manifest, string folder, bool merge = false)
        {
            string path = Path.Combine(folder, DiskNames.Manifest);
            Write(manifest, path, merge);
        }

        public static bool ValidPackageName(string name)
        {
            char[] invalidchars = new char[] { '/', '\\' };
            int i = name.IndexOfAny(invalidchars);
            bool valid = i == -1;
            return valid;
        }

        public static string CleanPackageName(string name)
        {
            var builder = new StringBuilder();
            foreach (char c in name)
            {
                if (c >= 'A' && c <= 'z') builder.Append(c);
            }
            return builder.ToString();
        }

    }

    public static class LockFile
    { 
        public static PackageAssets Read(string path)
        {
            if (File.Exists(path))
            {
                var content = File.ReadAllText(path);
                var dto = Parser.ReadAssets(content);
                return new PackageAssets
                {
                    Refs = dto.PackageReferences.ToPackageReferences(),
                    Missing = dto.MissingReferences.ToPackageReferences()
                };

            }
            else return null;
        }

        public static void Write(Assets assets, string path)
        {
            assets.Updated = DateTime.Now;
            var content = Parser.WriteAssets(assets);
            File.WriteAllText(path, content);
        }

        public static bool Outdated(string folder)
        {
            var man_path = Path.Combine(folder, DiskNames.Manifest);
            var man_time = File.GetLastWriteTimeUtc(man_path);

            var asset_path = Path.Combine(folder, DiskNames.Assets);
            var asset_time = File.GetLastWriteTimeUtc(asset_path);
            return asset_time < man_time;
        }

        public static PackageAssets ReadFromFolder(string folder)
        {
            var path = Path.Combine(folder, DiskNames.Assets);
            return Read(path);
        }

        public static PackageAssets ReadFromFolderOrCreate(string folder)
        {
            var path = Path.Combine(folder, DiskNames.Assets);
            if (File.Exists(path))
            {
                return Read(path);
            }
            else
            {
                return new PackageAssets();
            }
        }


        public static void WriteToFolder(PackageAssets assets, string folder)
        {
            var dto = assets.CreateAssetsFile();
            var path = Path.Combine(folder, DiskNames.Assets);
            Write(dto, path);
        }
    }

    public static class CanonicalIndexFile
    {
        public static void Write(CanonicalReferences references, string path)
        {
            var content = Parser.WriteReferences(references);
            File.WriteAllText(path, content);
        }

        public static CanonicalReferences Read(string path)
        {
            if (File.Exists(path))
            {
                var content = File.ReadAllText(path);
                return Parser.ReadReferences(content);

            }
            else return null;
        }

        public static CanonicalReferences ReadFromFolder(string folder)
        {
            var path = Path.Combine(folder, DiskNames.References);
            return Read(path);
        }

        public static void WriteToFolder(CanonicalReferences references, string folder)
        {
            var path = Path.Combine(folder, DiskNames.References);
            Write(references, path);
        }

        public static string GetCanonicalFromFile(string filepath)
        {
            try
            {
                var navigator = ElementNavigation.GetNavigatorForFile(filepath);
                var canonical = (string)navigator.Scalar("url");
                return canonical;
            }
            catch
            {
                return null;
            }
        }

        public static Dictionary<string, string> GetCanonicalsFromFiles(IEnumerable<string> filepaths)
        {
            var dictionary = new Dictionary<string, string>();
            foreach (var filepath in filepaths)
            {
                var canonical = GetCanonicalFromFile(filepath);
                if (canonical != null)
                {
                    var filename = Path.GetFileName(filepath);
                    dictionary[canonical] = filename;
                }
            }
            return dictionary;
        }

        public static Dictionary<string, string> GetIndexFromFolderContents(string folder)
        {
            var filenames = Directory.GetFiles(folder);
            return GetCanonicalsFromFiles(filenames);
        }
    }


    public static class Disk
    {
    
        public static string GetFolderName(string path)
        {
            return new DirectoryInfo(path).Name;
        }
        
     }
}


