using System.IO;
using System.Collections.Generic;
using System.Linq;
using Hl7.FhirPath;
using System;
using System.Text;

namespace Hl7.Fhir.Packages
{

    public static class Disk
    {
        public static PackageManifest ReadManifest(string path)
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

        public static void WriteManifest(PackageManifest manifest, string path, bool merge = false)
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

        public static PackageAssets ReadAssets(string path)
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

        public static void WriteAssets(AssetsFile assets, string path)
        {
            assets.Updated = DateTime.Now;
            var content = Parser.WriteAssets(assets);
            File.WriteAllText(path, content);
        }

        public static void WriteReferences(CanonicalReferences references, string path)
        {
            var content = Parser.WriteReferences(references);
            File.WriteAllText(path, content);
        }

        public static CanonicalReferences ReadReferences(string path)
        {
            if (File.Exists(path))
            {
                var content = File.ReadAllText(path);
                return Parser.ReadReferences(content);

            }
            else return null;
        }

        public static PackageManifest ReadFolderManifest(string folder)
        {
            var path = Path.Combine(folder, DiskNames.Manifest);
            return ReadManifest(path);
        }

        public static bool AssetsOutdated(string folder)
        {
            var man_path = Path.Combine(folder, DiskNames.Manifest);
            var man_time = File.GetLastWriteTimeUtc(man_path); 

            var asset_path = Path.Combine(folder, DiskNames.Assets);
            var asset_time = File.GetLastWriteTimeUtc(asset_path);
            return asset_time < man_time;
        }

        public static PackageManifest ReadOrCreateFolderManifest(string folder)
        {
            var manifest = ReadFolderManifest(folder);
            if (manifest is null)
            {
                var name = CleanPackageName(GetFolderName(folder));
                manifest = CreateManifest(name);
            }
            return manifest;
        }

        public static PackageManifest CreateManifest(string name)
        {
            return new PackageManifest
            {
                Name = name,
                Description = "Put a description here",
                Version = "0.1.0",
                Dependencies = new Dictionary<string, string>()
            };
        }

        public static string CleanPackageName(string name)
        {
            var builder = new StringBuilder();
            foreach(char c in name)
            {
                if (c >= 'A' && c <= 'z') builder.Append(c);
            }
            return builder.ToString();
        }

        public static CanonicalReferences ReadFolderReferences(string folder)
        {
            var path = Path.Combine(folder, DiskNames.References);
            return ReadReferences(path);
        }

        public static void WriteFolderReferences(CanonicalReferences references, string folder)
        {
            var path = Path.Combine(folder, DiskNames.References);
            WriteReferences(references, path);
        }

        public static PackageAssets ReadFolderAssets(string folder)
        {
            var path = Path.Combine(folder, DiskNames.Assets);
            return ReadAssets(path);
        }

        public static PackageAssets ReadOrCreateFolderAssets(string folder)
        {
            var path = Path.Combine(folder, DiskNames.Assets);
            if (File.Exists(path))
            {
                return ReadAssets(path);
            }
            else
            {
                return new PackageAssets();
            }
        }

        public static void WriteFolderAssets(PackageAssets assets, string folder)
        {
            var dto = assets.CreateAssetsFile();
            var path = Path.Combine(folder, DiskNames.Assets);
            WriteAssets(dto, path);
        }

        public static void WriteFolderManifest(PackageManifest manifest, string folder, bool merge = false)
        {
            string path = Path.Combine(folder, DiskNames.Manifest);
            WriteManifest(manifest, path, merge);
        }
     
        public static string GetFolderName(string path)
        {
            return new DirectoryInfo(path).Name;
        }

        public static bool ValidPackageName(string name)
        {
            char[] invalidchars = new char[] { '/', '\\' };
            int i = name.IndexOfAny(invalidchars);
            bool valid = i == -1;
            return valid;
        }

        public static string GetCanonicalFromFile(string filepath)
        {
            try
            {
                var navigator = Nav.GetNavigatorForFile(filepath);
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
                var canonical = Disk.GetCanonicalFromFile(filepath);
                if (canonical != null)
                {
                    var filename = Path.GetFileName(filepath);
                    dictionary[canonical] = filename;
                    //dictionary.Add(canonical, filename);
                }
            }
            return dictionary;
        }

        public static Dictionary<string, string> GetIndexFromFolderContents(string folder)
        {
            var filenames = Directory.GetFiles(folder);
            return Disk.GetCanonicalsFromFiles(filenames);
        }

    }
}


