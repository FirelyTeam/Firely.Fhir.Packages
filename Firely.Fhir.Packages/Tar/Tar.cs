using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Firely.Fhir.Packages
{
    public static class Tar
    {
        public static string PackToDisk(IEnumerable<FileEntry> entries, string path)
        {
            var packagefile = Path.ChangeExtension(path, ".tgz");

            using var file = File.Create(packagefile);
            using var gzip = new GZipOutputStream(file);
            using TarOutputStream tar = new TarOutputStream(gzip);
            
            Write(tar, entries);
            
            return packagefile;
        }

        public static byte[] Pack(IEnumerable<FileEntry> entries)
        {
            var stream = new LateDisposalMemoryStream();
            using (var gzip = new GZipOutputStream(stream))
            using (TarOutputStream tar = new TarOutputStream(gzip))
            {
                tar.Write(entries);
            }
            stream.Seek(0, SeekOrigin.Begin);
            var bytes = stream.ToArray();
            stream.DisposeAfter();
            return bytes;
        }

        public static void ExtractTarballToDisk(byte[] buffer, string folder)
        {
            var tar = Tar.Unzip(buffer);
            Tar.ExtractTarArchiveToDisk(tar, folder);
        }

        internal static void ExtractTarArchiveToDisk(byte[] buffer, string folder)
        {
            Directory.CreateDirectory(folder);
            var stream = new MemoryStream(buffer);

            using var archive = TarArchive.CreateInputTarArchive(stream);

            archive.ExtractContents(folder);
        }
        
        internal static IEnumerable<FileEntry> ExtractFiles(Stream stream)
        {
            return ExtractFiles(stream, _ => true);
        }

        internal static IEnumerable<FileEntry> ExtractFiles(string path)
        {
            return ExtractFiles(path, _ => true);
        }

        internal static IEnumerable<FileEntry> ExtractFiles(string path, Predicate<string> predicate)
        {
            using var file = File.OpenRead(path);

            foreach (FileEntry entry in ExtractFiles(file, predicate))
            {
                yield return entry;
            }
        }

        internal static IEnumerable<FileEntry> ExtractFiles(Stream stream, Predicate<string> predicate)
        {
            using var gzip = new GZipInputStream(stream);
            using var tar = new TarInputStream(gzip);

            for (TarEntry tarEntry = tar.GetNextEntry(); tarEntry != null; tarEntry = tar.GetNextEntry())
            {
                if (predicate(tarEntry.Name))
                {
                    var fileEntry = new FileEntry
                    {
                        FilePath = tarEntry.Name
                    };

                    using (var output = new MemoryStream())
                    {
                        tar.CopyEntryContents(output);
                        fileEntry.Buffer = output.ToArray();
                    }
                    yield return fileEntry;
                }
            }
        }

        internal static byte[] Unzip(byte[] buffer)
        {
            using var instream = new MemoryStream(buffer);
            using var outstream = new MemoryStream();

            GZip.Decompress(instream, outstream, false);
            return outstream.ToArray();
        }

        public static void PackToStream(IEnumerable<FileEntry> entries, Stream stream)
        {
            using var gzip = new GZipOutputStream(stream);
            using TarOutputStream tar = new TarOutputStream(gzip);

            Tar.Write(tar, entries);
        }

        [CLSCompliant(false)]
        public static void Write(this TarOutputStream tar, IEnumerable<FileEntry> entries)
        {
            foreach (var entry in entries)
            {
                Write(tar, entry);
            }
        }

        [CLSCompliant(false)]
        public static void Write(this TarOutputStream tar, FileEntry file)
        {
            using (Stream stream = file.GetStream())
            {
                long size = stream.Length;
                var path = file.FilePath.Replace('\\', '/');
                TarEntry entry = TarEntry.CreateTarEntry(path);
                entry.Size = size;
                tar.PutNextEntry(entry);

                byte[] localBuffer = new byte[32 * 1024];
                while (true)
                {
                    int numRead = stream.Read(localBuffer, 0, localBuffer.Length);
                    if (numRead <= 0)
                    {
                        break;
                    }
                    tar.Write(localBuffer, 0, numRead);
                }
            }
            tar.CloseEntry();
        }

        [CLSCompliant(false)]
        public static void Write(this TarOutputStream tar, string path, string content)
        {
            var entry = new FileEntry
            {
                FilePath = path,
                Buffer = Encoding.UTF8.GetBytes(content)
            };
            Write(tar, entry);
        }

        
    }

}


