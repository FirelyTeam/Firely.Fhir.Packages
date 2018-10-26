using System.IO;
using ICSharpCode.SharpZipLib.Tar;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Hl7.Fhir.Packages
{

    public static partial class Packaging
    {
        const string PACKAGE = "package";

        public static PackageManifest ExtractManifestFromPackageFile(string path)
        {
            string file = Path.Combine(PACKAGE, DiskNames.Manifest);
            var entry = Tar.ExtractMatchingFiles(path, file).FirstOrDefault();
            return Parser.ReadManifest(entry.Buffer);
        }

        public static void WriteResource(this TarOutputStream tar, FileEntry entry)
        {
            entry.ChangeFolder(PACKAGE);
            Tar.Write(tar, entry);
        }

        public static string PackFolder(string name, string folder)
        {
            var manifest = FileEntries.ReadFileEntry(Path.Combine(folder, DiskNames.Manifest)).ChangeFolder(PACKAGE);
            var entries = FileEntries.ReadFilesToPack(folder).ChangeFolder(PACKAGE);

            return Tar.PackToDisk(name, manifest, entries);
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
                FilePath = Path.Combine(PACKAGE, DiskNames.Manifest),
                Buffer = manifest.ToByteArray()
            };
        }

        public static byte[] CreatePackage(PackageManifest manifest, IEnumerable<FileEntry> entries)
        {
            var entry = manifest.ToFileEntry();
            entries = entries.ChangeFolder(PACKAGE);
            return Tar.Pack(entry, entries);
        }

        public static byte[] CreatePackage(IEnumerable<FileEntry> entries)
        {
            entries = entries.ChangeFolder(PACKAGE);
            return Tar.Pack(entries);
        }

        public static void UnpackToFolder(byte[] buffer, string folder) //
        {
            var tarball = Tar.Unzip(buffer);
            Tar.ExtractTarballToToDisk(tarball, folder);
        }

        public static (PackageManifest manifest, IEnumerable<string> files) GetPackageSummary(this IEnumerable<FileEntry> entries)
        {
            var files = new List<string>();
            var manifest = new PackageManifest();
            foreach (var entry in entries)
            {
                var filename = Path.GetFileName(entry.FilePath);
                if (filename == DiskNames.Manifest)
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

        public static IEnumerable<FileEntry> ExtractFiles(string path)
        {
            return Tar.ExtractFiles(path, _ => true);
        }

        public static IEnumerable<FileEntry> ExtractFiles(Stream stream)
        {
            return Tar.ExtractFiles(stream, _ => true);
        }
    }
}


