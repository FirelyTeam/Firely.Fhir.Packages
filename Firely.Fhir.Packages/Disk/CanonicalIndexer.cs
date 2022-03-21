using Hl7.Fhir.ElementModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Firely.Fhir.Packages
{

    public static class CanonicalIndexer
    {
        public const int VERSION = 5;

        /// <summary>
        /// Builds a canonical index from a list of metadata entries.
        /// </summary>
        public static CanonicalIndex BuildCanonicalIndex(IEnumerable<ResourceMetadata> entries)
        {
            var index = new CanonicalIndex { Files = new(), Version = VERSION, date = DateTimeOffset.Now };
            return index.Append(entries);
        }

        /// <summary>
        /// Appends Metadata to a canonical index.
        /// </summary>
        public static CanonicalIndex Append(this CanonicalIndex index, IEnumerable<ResourceMetadata> entries)
        {
            index.Files.AddRange(entries);
            return index;
        }

        /// <summary>
        /// Creates the metadata entries for a CanonicalIndex for a folder
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="recurse"></param>
        /// <returns></returns>
        public static List<ResourceMetadata> IndexFolder(string folder, bool recurse)
        {
            var option = recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var paths = Directory.GetFiles(folder, "*.*", option);
            return EnumerateFolderMetadata(folder, paths).ToList();
        }

        private static IEnumerable<ResourceMetadata> EnumerateFolderMetadata(string folder, IEnumerable<string> filepaths)
        {
            foreach (var filepath in filepaths)
            {
                var meta = GetFileMetadata(folder, filepath);
                if (meta is object)
                    yield return meta;
            }
        }


        /// <summary>
        /// Builds the CanonicalIndex Resource metadata for a single file on disk
        /// </summary>
        public static ResourceMetadata? GetFileMetadata(string folder, string filepath)
        {
            try
            {
                var node = FhirParser.ParseFileToSourceNode(filepath);
                if (node is null) return null;

                string? canonical = node.GetString("url"); // node.Children("url").FirstOrDefault()?.Text;

                return new ResourceMetadata
                {
                    FileName = GetRelativePath(folder, filepath),
                    ResourceType = node.Name,
                    Id = node.GetString("id"),
                    Canonical = node.GetString("url"),
                    Version = node.GetString("version"),
                    Kind = node.GetString("kind"),
                    Type = node.GetString("type"),
                    FhirVersion = node.GetString("fhirVersion")
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Builds Metadata for a Package File Index for a single file.
        /// </summary>
        public static ResourceMetadata BuildResourceMetadata(string filename, ISourceNode resource)
        { 
            return new ResourceMetadata
            {
                FileName = filename,
                ResourceType = resource.Name,
                Id = resource.GetString("id"),
                Canonical = resource.GetString("url"),
                Version = resource.GetString("version"),
                Kind = resource.GetString("kind"),
                Type = resource.GetString("type"),
                FhirVersion = resource.GetString("fhirVersion")
            };
        }

        public static string GetString(this ISourceNode node, string expression)
        {
            if (node is null) return null;

            var parts = expression.Split('.');

            foreach (var part in parts)
            {
                node = node.Children(part).FirstOrDefault();
                if (node is null) return null;
            }
            return node.Text;
        }

        public static IEnumerable<string> GetRelativePaths(string folder, IEnumerable<string> paths)
        {
            foreach (var path in paths)
                yield return GetRelativePath(folder, path);
        }

        private static string DirectorySeparatorString = $"{Path.DirectorySeparatorChar}";

        public static string GetRelativePath(string relativeTo, string path)
        {
          
            // Require trailing backslash for path
            if (!relativeTo.EndsWith(DirectorySeparatorString)) 
                relativeTo += DirectorySeparatorString;

            Uri baseUri = new Uri(relativeTo);
            Uri fullUri = new Uri(path);
            
            Uri relativeUri = baseUri.MakeRelativeUri(fullUri);

            // Uri's use forward slashes so convert back to backward slashes
            var result = Uri.UnescapeDataString(relativeUri.ToString());
            return result;

        }
    }
}


