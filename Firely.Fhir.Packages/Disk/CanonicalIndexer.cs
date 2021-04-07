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
            return EnumerateMetadata(folder, paths).ToList();
        }

        private static IEnumerable<ResourceMetadata> EnumerateMetadata(string folder, IEnumerable<string> filepaths)
        {
            foreach (var filepath in filepaths)
            {
                var meta = GetFileMetadata(folder, filepath);
                if (meta is object)
                    yield return meta;
            }
        }

        public static ResourceMetadata? GetFileMetadata(string folder, string filepath)
        {
            try
            {
                var node = ElementNavigation.ParseToSourceNode(filepath);
                if (node is null) return null;

                string? canonical = node.GetString("url"); // node.Children("url").FirstOrDefault()?.Text;

                return new ResourceMetadata
                {
                    FileName = Disk.GetRelativePath(filepath, folder),
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

        public static IEnumerable<string> GetRelativePaths(string root, IEnumerable<string> paths)
        {
            foreach (var path in paths)
                yield return Disk.GetRelativePath(path, root);
        }

       
    }
}


