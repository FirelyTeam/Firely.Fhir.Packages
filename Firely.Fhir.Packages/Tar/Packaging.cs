using ICSharpCode.SharpZipLib.Tar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{

    public static class Packaging
    {
        const string PACKAGEFOLDER = "package";

        public static PackageManifest ExtractManifestFromPackageFile(string path)
        {
            string file = Path.Combine(PACKAGEFOLDER, PackageConsts.Manifest);
            var entry = ExtractMatchingFiles(path, file).FirstOrDefault();
            return Parser.ReadManifest(entry.Buffer);
        }

        [CLSCompliant(false)]
        public static void WriteResource(this TarOutputStream tar, FileEntry entry)
        {
            entry.ChangeFolder(PACKAGEFOLDER);
            Tar.Write(tar, entry);
        }

        public static string PackFolder(string name, string folder)
        {
            var files = FileEntries
                .ReadAllFilesToPack(folder)
                .MakePathsRelative(folder)
                .OrganizeToPackageStructure();

            return Tar.PackToDisk(files, name);
        }


        public static byte[] ToByteArray(this PackageManifest manifest)
        {
            var content = Parser.WriteManifest(manifest);
            return Encoding.ASCII.GetBytes(content);
        }

        public static FileEntry ToFileEntry(this PackageManifest manifest)
        {
            return new FileEntry
            {
                FilePath = Path.Combine(PACKAGEFOLDER, PackageConsts.Manifest),
                Buffer = manifest.ToByteArray()
            };
        }

        //public static byte[] CreatePackage(PackageManifest manifest, IEnumerable<FileEntry> entries)
        //{
        //    var entry = manifest.ToFileEntry();
        //    entries = entries.ChangeFolder(PACKAGE);
        //    return Tar.Pack(entry, entries);
        //}

        public static byte[] CreatePackage(IEnumerable<FileEntry> entries)
        {
            entries = entries.ChangeFolder(PACKAGEFOLDER);
            return Tar.Pack(entries);
        }

        public static async Task ExtractPackageToDisk(byte[] buffer, string folder)
        {
            await Task.Run(() =>
            {
                var tar = Tar.Unzip(buffer);
                Tar.ExtractTarArchiveToDisk(tar, folder);
            });
        }

        public static (PackageManifest manifest, IEnumerable<string> files) GetPackageSummary(this IEnumerable<FileEntry> entries)
        {
            var files = new List<string>();
            var manifest = new PackageManifest();
            foreach (var entry in entries)
            {
                var filename = Path.GetFileName(entry.FilePath);
                if (filename == PackageConsts.Manifest)
                {
                    manifest = Parser.ReadManifest(entry.Buffer);
                }
                else
                {
                    files.Add(entry.FilePath);
                }
            }

            return (manifest, files);
        }

        public static IEnumerable<FileEntry> ExtractMatchingFiles(string packagefile, string match)
        {
            return Tar.ExtractFiles(packagefile, name => Disk.PathMatch(name, match));
        }

    }
}


