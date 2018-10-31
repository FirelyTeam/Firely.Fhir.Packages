using System.IO;
using System.Collections.Generic;
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
}


