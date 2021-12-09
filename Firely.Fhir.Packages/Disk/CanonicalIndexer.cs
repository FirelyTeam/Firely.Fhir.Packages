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
        public static List<ResourceMetadata> IndexFolder(string folder, bool recurse)
        {
            var option = recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var paths = Directory.GetFiles(folder, "*.*", option);
            return enumerateMetadata(folder, paths).ToList();
        }

        private static IEnumerable<ResourceMetadata> enumerateMetadata(string folder, IEnumerable<string> filepaths)
        {
            return filepaths.Select(p => GetFileMetadata(folder, p)).Where(p => p is not null);
        }

        public static ResourceMetadata GetFileMetadata(string folder, string filepath)
        {
            return ElementNavigation.TryParseToSourceNode(filepath, out var node)
                    ? new ResourceMetadata(filename: Path.GetFileName(filepath), filepath: GetRelativePath(folder, filepath))
                    {
                        ResourceType = node?.Name,
                        Id = node.GetString("id"),
                        Canonical = node.GetString("url"),
                        Version = node.GetString("version"),
                        Kind = node.GetString("kind"),
                        Type = node.GetString("type"),
                        FhirVersion = node.GetString("fhirVersion"),
                        HasSnapshot = node?.checkForSnapshot(),
                        HasExpansion = node?.checkForExpansion(),
                        ValueSetCodeSystem = node?.getCodeSystemFromValueSet(),
                        NamingSystemUniqueId = node?.getUniqueIdsFromNamingSystem(),
                        ConceptMapUris = node?.getConceptMapsSourceAndTarget()
                    }
                    : new ResourceMetadata(filename: Path.GetFileName(filepath), filepath: GetRelativePath(folder, filepath));
        }

        internal static IEnumerable<IndexData> GenerateIndexFile(IEnumerable<FileEntry> entries)
        {
            return entries.Select(e => getIndexData(e)).Where(e => e is not null);
        }

        private static IndexData getIndexData(FileEntry entry)
        {
            return ElementNavigation.TryParseToSourceNode(entry.GetStream(), out var node)
                  ? new IndexData(filename: Path.GetFileName(entry.FilePath))
                  {
                      ResourceType = node?.Name,
                      Id = node.GetString("id"),
                      Canonical = node.GetString("url"),
                      Version = node.GetString("version"),
                      Kind = node.GetString("kind"),
                      Type = node.GetString("type")
                  }
                : new IndexData(filename: Path.GetFileName(entry.FilePath));
        }


        public static string? GetString(this ISourceNode? node, string expression)
        {
            if (node is null) return null;
            node = node.findFirstDescendant(expression);
            return node?.Text;
        }

        public static IEnumerable<string> GetRelativePaths(string folder, IEnumerable<string> paths)
        {
            foreach (var path in paths)
                yield return GetRelativePath(folder, path);
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

        public static string GetRelativePath(string relativeTo, string path)
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
                var uri = node.GetString("compose.include.system");
                string? version;

                if (uri is not null)
                    version = node.GetString("compose.include.version");
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
                       ?.Select(node => node.GetString("value"))
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
                    TargetUri = node.GetString("targetCanonical") ?? node.GetString("targetUri") ?? node.GetString("targetReference.reference"),
                    SourceUri = node.GetString("sourceCanonical") ?? node.GetString("sourceUri") ?? node.GetString("sourceReference.reference")
                }
                : null;
        }



    }
}


#nullable restore