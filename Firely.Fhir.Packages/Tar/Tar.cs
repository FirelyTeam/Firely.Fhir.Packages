/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


#nullable enable

using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Firely.Fhir.Packages
{
    internal static class Tar
    {
        internal static string PackToDisk(string path, IEnumerable<FileEntry> entries)
        {

            var packagefile = Path.ChangeExtension(path, ".tgz");

            using var file = File.Create(packagefile);
            using var gzip = new GZipOutputStream(file);
            using TarOutputStream tar = new TarOutputStream(gzip);

            Write(tar, entries);

            return packagefile;
        }

        internal static string PackToDisk(string path, FileEntry single, IEnumerable<FileEntry> entries)
        {

            var packagefile = Path.ChangeExtension(path, ".tgz");

            using (var file = File.Create(packagefile))
            using (var gzip = new GZipOutputStream(file))
            using (TarOutputStream tar = new TarOutputStream(gzip))
            {
                Write(tar, single);
                Write(tar, entries);
            }
            return packagefile;
        }

        internal static byte[] Pack(FileEntry single, IEnumerable<FileEntry> entries)
        {
            var stream = new LateDisposalMemoryStream();
            using (var gzip = new GZipOutputStream(stream))
            using (TarOutputStream tar = new TarOutputStream(gzip))
            {
                tar.Write(single);
                tar.Write(entries);
            }
            stream.Seek(0, SeekOrigin.Begin);
            var bytes = stream.ToArray();
            stream.DisposeAfter();
            return bytes;
        }

        internal static byte[] Pack(IEnumerable<FileEntry> entries)
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

        //public static void WriteManifest(TarOutputStream tar, PackageManifest manifest)
        //{
        //    var path = Path.Combine(PACKAGE, DiskNames.Manifest);
        //    var content = JsonConvert.SerializeObject(manifest, Formatting.Indented);
        //    Internal.Write(tar, path, content);
        //}

        internal static void ExtractTarballToToDisk(byte[] buffer, string folder)
        {
            Directory.CreateDirectory(folder);
            var stream = new MemoryStream(buffer);

            using var archive = TarArchive.CreateInputTarArchive(stream);

            archive.ExtractContents(folder);
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
                    using var output = new MemoryStream();
                    string filePath = tarEntry.Name;
                    tar.CopyEntryContents(output);
                    var buffer = output.ToArray();

                    yield return new FileEntry(filePath, buffer);
                }
            }
        }

        internal static IEnumerable<FileEntry> ExtractMatchingFiles(string packagefile, string match)
        {
            return ExtractFiles(packagefile, name => PathMatch(name, match));
        }

        internal static bool PathMatch(string pathA, string pathB)
        {
            pathA = pathA.Replace('\\', '/');
            pathB = pathB.Replace('\\', '/');
            return pathA == pathB;
        }

        internal static byte[] Unzip(byte[] buffer)
        {
            using var instream = new MemoryStream(buffer);
            using var outstream = new MemoryStream();

            GZip.Decompress(instream, outstream, false);
            return outstream.ToArray();
        }

        internal static void PackToStream(IEnumerable<FileEntry> entries, Stream stream)
        {
            using var gzip = new GZipOutputStream(stream);
            using TarOutputStream tar = new TarOutputStream(gzip);

            Tar.Write(tar, entries);
        }


        internal static void Write(this TarOutputStream tar, IEnumerable<FileEntry> entries)
        {
            foreach (var entry in entries)
            {
                Write(tar, entry);
            }
        }

        internal static void Write(this TarOutputStream tar, FileEntry file)
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

        internal static void Write(this TarOutputStream tar, string path, string content)
        {
            var entry = new FileEntry(path, Encoding.UTF8.GetBytes(content));
            Write(tar, entry);
        }
    }
}

#nullable restore  


