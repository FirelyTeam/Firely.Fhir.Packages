#nullable enable

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
                yield return readFileEntry(filepath);
            }
        }


        private static bool match(this FileEntry file, string filename)
        {
            return string.Compare(file.FileName, filename, ignoreCase: true) == 0;
        }

        private static bool hasExtension(this FileEntry file, params string[] extensions)
        {
            var extension = Path.GetExtension(file.FileName);
            foreach (var ext in extensions)
            {
                if (string.Compare(extension, ext, ignoreCase: true) == 0) return true;
            }
            return false;
        }

        private static IEnumerable<string> allFilesToPack(string folder)
        {
            return Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
        }

        internal static IEnumerable<FileEntry> ReadAllFilesToPack(string folder)
        {
            return allFilesToPack(folder).Select(readFileEntry);
        }

        /// <summary>
        /// Indexes a list of file entries and adds that index file to the list
        /// </summary>
        /// <param name="entries">File entries to be indexed</param>
        /// <returns>List of file entries including an index file</returns>
        public static IEnumerable<FileEntry> AddIndexFiles(this IEnumerable<FileEntry> entries)
        {
            var entryList = entries.ToList();
            var folders = entries.Select(e => Path.GetDirectoryName(e.FilePath)).Where(f => f is not null).Distinct();

            foreach (var folder in folders)
            {
                var files = entries.Where(e => Path.GetDirectoryName(e.FilePath) == folder);
                var indexFile = IndexJsonFile.GenerateIndexFile(files, folder!);
                entryList.Add(indexFile);
            }

            return entryList;
        }

        private static FileEntry readFileEntry(string filepath)
        {
            var buffer = File.ReadAllBytes(filepath);
            var entry = new FileEntry(filepath, buffer);
            return entry;
        }

        internal static IEnumerable<FileEntry> ChangeFolder(this IEnumerable<FileEntry> entries, string folder)
        {
            foreach (var entry in entries)
            {
                yield return entry.ChangeFolder(folder);
            }
        }

        internal static FileEntry ChangeFolder(this FileEntry entry, string folder)
        {
            string filename = Path.GetFileName(entry.FilePath);
            entry.FilePath = Path.Combine(folder, Path.GetFileName(filename));
            return entry;
        }

        internal static IEnumerable<FileEntry> MakePathsRelative(this IEnumerable<FileEntry> files, string root)
        {
            foreach (var file in files)
                yield return file.MakeRelativePath(root);
        }


        private static Uri makeFolderUri(string folder)
        {
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            return new Uri(folder);
        }

        private static string makeRelativePath(string path, string root)
        {
            Uri pathUri = new Uri(path);
            Uri rootUri = makeFolderUri(root);
            return Uri.UnescapeDataString(rootUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        /// <summary>
        /// Get the relative path of the file entry opposed to the root of the package
        /// </summary>
        /// <param name="file">Current file entry</param>
        /// <param name="root">Package root path</param>
        /// <returns>the relative path of the file entry opposed to the root of the package</returns>
        public static FileEntry MakeRelativePath(this FileEntry file, string root)
        {
            file.FilePath = makeRelativePath(file.FilePath, root);
            return file;
        }

        private static readonly string FOLDER_OTHER = Path.Combine(PackageFileNames.PACKAGEFOLDER, "other");

        /// <summary>
        /// This is a basic implementation to move the package manifest and all resources to the package folder and 
        /// all other files to packages/other. You can write and inject your own implementation when packaging a folder
        /// </summary>
        /// <param name="file"></param>
        /// <returns>An organized file entry</returns>
        public static FileEntry OrganizeToPackageStructure(this FileEntry file)
        {
            if (file.match(PackageFileNames.MANIFEST))
                return file.ChangeFolder(PackageFileNames.PACKAGEFOLDER);

            else if (file.hasExtension(".xml", ".json"))
                return file.ChangeFolder(PackageFileNames.PACKAGEFOLDER);

            else
                return file.ChangeFolder(FOLDER_OTHER);
        }
    }
}

#nullable restore


