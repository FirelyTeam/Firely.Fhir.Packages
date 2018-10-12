using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Hl7.Fhir.Packages
{
    public static class FileEntries
    {
        public static IEnumerable<FileEntry> ReadFileEntries(string folder, string pattern)
        {
            foreach (var filepath in Directory.GetFiles(folder, pattern))
            {
                yield return ReadFileEntry(filepath);
            }
        }

        public static bool IsManifestFile(string filepath)
        {
            return Path.GetFileName(filepath).ToLower() == DiskNames.Manifest;
        }

        public static IEnumerable<string> FilesToPack(string folder)
        {
            foreach (var filepath in Directory.GetFiles(folder, "*.xml"))
                yield return filepath;

            foreach (var filepath in Directory.GetFiles(folder, "*.json").Where(f => !IsManifestFile(f)))
                yield return filepath;
        }

        public static IEnumerable<FileEntry> ReadFilesToPack(string folder)
        {
            return FilesToPack(folder).Select(ReadFileEntry);
        }

        public static FileEntry ReadFileEntry(string filepath)
        {
            var buffer = File.ReadAllBytes(filepath);
            var entry = new FileEntry { FilePath = filepath, Buffer = buffer };
            return entry;
        }

        public static IEnumerable<FileEntry> ChangeFolder(this IEnumerable<FileEntry> entries, string folder)
        {
            foreach (var entry in entries)
            {
                yield return entry.ChangeFolder(folder);    
            }
        }

        public static FileEntry ChangeFolder(this FileEntry entry, string folder)
        {
            string filename = Path.GetFileName(entry.FilePath);
            entry.FilePath = Path.Combine(folder, Path.GetFileName(filename));
            return entry;
        }

    }

}


