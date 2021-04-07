using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Firely.Fhir.Packages
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

        public static bool Match(this FileEntry file, string filename)
        {
            return string.Compare(file.FileName, filename, ignoreCase: true) == 0;
        }

        public static bool HasExtension(this FileEntry file, params string[] extensions)
        {
            var extension = Path.GetExtension(file.FileName);
            foreach(var ext in extensions)
            {
                if (string.Compare(extension, ext, ignoreCase: true) == 0) return true;
            }
            return false;
        }

        public static IEnumerable<string> AllFilesToPack(string folder)
        {
            return Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
        }

        public static IEnumerable<FileEntry> ReadAllFilesToPack(string folder)
        {
            return AllFilesToPack(folder).Select(ReadFileEntry);
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

        public static IEnumerable<FileEntry> MakePathsRelative(this IEnumerable<FileEntry> files, string root)
        {
            foreach (var file in files)
                yield return file.MakePathRelativeTo(root);
        }



        public static IEnumerable<FileEntry> OrganizeToPackageStructure(this IEnumerable<FileEntry> files)
        {
            var other = Path.Combine(PackageConsts.PackageFolder, "other");

            foreach (var file in files)
            {
                if (file.Match(PackageConsts.Manifest))
                    yield return file.ChangeFolder(PackageConsts.PackageFolder);

                if (file.HasExtension(".xml", ".json"))
                    yield return file.ChangeFolder(PackageConsts.PackageFolder);

              
                yield return file.ChangeFolder(other);
            }
        }

        public static FileEntry MakePathRelativeTo(this FileEntry file, string root)
        {
            file.FilePath = Disk.GetRelativePath(file.FilePath, root);
            return file;
        }

    }

}


