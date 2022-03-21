#nullable enable

using Hl7.Fhir.ElementModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Firely.Fhir.Packages
{

    public static class CanonicalIndexer
    {
        public const int FIRELY_INDEX_VERSION = 7;
        public const int INDEX_JSON_VERSION = 1;

        /// <summary>
        /// Builds a canonical index (.firely.index.json) from a list of metadata entries.
        /// </summary>
        public static CanonicalIndex BuildCanonicalIndex(IEnumerable<ResourceMetadata> entries)
        {
            var index = new CanonicalIndex { Files = new(), Version = FIRELY_INDEX_VERSION, date = DateTimeOffset.Now };
            return index.Append(entries);
        }

        /// <summary>
        /// Appends Metadata to a canonical index.
        /// </summary>
        public static CanonicalIndex Append(this CanonicalIndex index, IEnumerable<ResourceMetadata> entries)
        {
            if (index.Files == null)
                index.Files = new();

            index.Files.AddRange(entries);
            return index;
        }

        /// <summary>
        /// Builds a index.json (.index.json) from a list of metadata entries.
        /// </summary>
        public static IndexJson BuildIndexJson(IEnumerable<IndexData> entries)
        {
            var index = new IndexJson { Files = new(), Version = INDEX_JSON_VERSION, date = DateTimeOffset.Now };
            return index.Append(entries);
        }

        /// <summary>
        /// Appends index data to a index json
        /// </summary>
        public static IndexJson Append(this IndexJson index, IEnumerable<IndexData> entries)
        {
            if (index.Files == null)
                index.Files = new();

            index.Files.AddRange(entries);
            return index;
        }

        /// <summary>
        /// Creates the metadata entries for a CanonicalIndex for a folder
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="recurse"></param>
        /// <returns></returns>
        internal static List<ResourceMetadata> IndexFolder(string folder, bool recurse)
        {
            var option = recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var paths = Directory.GetFiles(folder, "*.*", option);
            return enumerateMetadata(folder, paths).ToList();
        }

        private static IEnumerable<ResourceMetadata> enumerateMetadata(string folder, IEnumerable<string> filepaths)
        {
            return filepaths.Select(p => getFileMetadata(folder, p)).Where(p => p is not null);
        }

        private static ResourceMetadata getFileMetadata(string folder, string filepath)
        {
            return FhirParser.TryParseToSourceNode(filepath, out var node)
                    ? BuildResourceMetadata(getRelativePath(folder, filepath), node!)
                    : new ResourceMetadata(filename: Path.GetFileName(filepath), filepath: getRelativePath(folder, filepath));
        }

        internal static IEnumerable<IndexData> GenerateIndexFile(IEnumerable<FileEntry> entries)
        {
            return entries.Select(e => getIndexData(e)).Where(e => e is not null);
        }

        private static IndexData getIndexData(FileEntry entry)
        {
            return FhirParser.TryParseToSourceNode(entry.GetStream(), out var node)
                  ? BuildIndex(Path.GetFileName(entry.FilePath), node!)
                : new IndexData(filename: Path.GetFileName(entry.FilePath));
        }

        /// <summary>
        /// Builds Firely specific metadata for a Package File Index for a single file.
        /// </summary>
        /// <param name="filepath">relative path of the file</param>
        /// <param name="resource">Resource to be indexed</param>
        /// <returns>An entry to .firely.index.json</returns>
        public static ResourceMetadata BuildResourceMetadata(string filepath, ISourceNode resource)
        {
            return new ResourceMetadata(filename: Path.GetFileName(filepath), filepath: filepath)
            {
                ResourceType = resource?.Name,
                Id = resource?.getString("id"),
                Canonical = resource?.getString("url"),
                Version = resource?.getString("version"),
                Kind = resource?.getString("kind"),
                Type = resource?.getString("type"),
                FhirVersion = resource?.getString("fhirVersion"),
                HasSnapshot = resource?.checkForSnapshot(),
                HasExpansion = resource?.checkForExpansion(),
                ValueSetCodeSystem = resource?.getCodeSystemFromValueSet(),
                NamingSystemUniqueId = resource?.getUniqueIdsFromNamingSystem(),
                ConceptMapUris = resource?.getConceptMapsSourceAndTarget()
            };
        }

        /// <summary>
        /// Builds index data for a Package File Index for a single file.
        /// </summary>
        /// <param name="filename">Name of the file</param>
        /// <param name="resource">Resource to be indexed</param>
        /// <returns>An entry to .index.json</returns>
        public static IndexData BuildIndex(string filename, ISourceNode resource)
        {
            return new IndexData(filename)
            {
                ResourceType = resource.Name,
                Id = resource.getString("id"),
                Canonical = resource.getString("url"),
                Version = resource.getString("version"),
                Kind = resource.getString("kind"),
                Type = resource.getString("type")
            };
        }


        private static string? getString(this ISourceNode node, string expression)
        {
            if (node is null) return null;
            var decendant = node.findFirstDescendant(expression);
            return decendant?.Text;
        }

        private static IEnumerable<string> getRelativePaths(string folder, IEnumerable<string> paths)
        {
            foreach (var path in paths)
                yield return getRelativePath(folder, path);
        }

        private static ISourceNode? findFirstDescendant(this ISourceNode? node, string expression)
        {
            var parts = expression.Split('.');

            foreach (var part in parts)
            {
                node = node?.Children(part).FirstOrDefault();
                if (node is null) return null;
            }

            return node;
        }

        private static IEnumerable<ISourceNode>? findDescendants(this ISourceNode node, string name)
        {
            return node.Descendants()?.Where(node => node.Name == name);
        }

        private static readonly string DIRECTORYSEPARATORSTRING = $"{Path.DirectorySeparatorChar}";

        private static string getRelativePath(string relativeTo, string path)
        {

            // Require trailing backslash for path
            if (!relativeTo.EndsWith(DIRECTORYSEPARATORSTRING))
                relativeTo += DIRECTORYSEPARATORSTRING;

            Uri baseUri = new(relativeTo);
            Uri fullUri = new(path);

            Uri relativeUri = baseUri.MakeRelativeUri(fullUri);

            // Uri's use forward slashes so convert back to backward slashes
            var result = Uri.UnescapeDataString(relativeUri.ToString());
            return result;

        }

        private static bool checkForSnapshot(this ISourceNode node)
        {
            return node.Name == "StructureDefinition" && node.Children("snapshot") is not null;
        }

        private static bool checkForExpansion(this ISourceNode node)
        {
            return node.Name == "ValueSet" && node.findFirstDescendant("expansion.contains") is not null;
        }

        private static string? getCodeSystemFromValueSet(this ISourceNode node)
        {
            if (node.isWholeCodeSystemValueSet())
            {
                var uri = node.getString("compose.include.system");
                string? version;

                if (uri is not null)
                    version = node.getString("compose.include.version");
                else
                    return null;

                return (version is not null) ? $"{uri}|{version}" : uri;

            }
            return null;
        }

        private static bool isWholeCodeSystemValueSet(this ISourceNode node)
        {
            return node.Name == "ValueSet" &&
                node.findFirstDescendant("compose.exclude") is null &&
                node.findFirstDescendant("compose.include.filter") is null &&
                node.findFirstDescendant("compose.include.concept") is null &&
                node.findFirstDescendant("compose.include.valueSet") is null;
        }

        private static string[]? getUniqueIdsFromNamingSystem(this ISourceNode node)
        {
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            return node.Name == "NamingSystem"
                ? node.findDescendants("uniqueId")
                       ?.Select(node => node.getString("value"))
                       ?.Where(s => s != null)
                       ?.ToArray()
                : null;
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
        }

        private static SourceAndTarget? getConceptMapsSourceAndTarget(this ISourceNode node)
        {
            return node.Name == "ConceptMap"
                ? new SourceAndTarget
                {
                    TargetUri = node.getString("targetCanonical") ?? node.getString("targetUri") ?? node.getString("targetReference.reference"),
                    SourceUri = node.getString("sourceCanonical") ?? node.getString("sourceUri") ?? node.getString("sourceReference.reference")
                }
                : null;
        }



    }
}


#nullable restore