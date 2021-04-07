using System;
using System.IO;

namespace Firely.Fhir.Packages
{
    internal static class Disk
    {
        internal static string GetFolderName(string path)
        {
            return new DirectoryInfo(path).Name;
        }

        internal static string GetRelativePath(string path, string root)
        {
            Uri pathUri = new Uri(path);
            Uri rootUri = GetFolderUri(root);
            return Uri.UnescapeDataString(rootUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        private static Uri GetFolderUri(string folder)
        {
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            return new Uri(folder);
        }

        internal static bool PathMatch(string pathA, string pathB)
        {
            pathA = pathA.Replace('\\', '/');
            pathB = pathB.Replace('\\', '/');
            return pathA == pathB;
        }

        // Duplicate found under CanonicalIndexer
        //public static string GetRelativePath(string root, string path)
        //{
        //    // Require trailing backslash for path
        //    if (!root.EndsWith("\\"))
        //        root += "\\";

        //    Uri baseUri = new Uri(root);
        //    Uri fullUri = new Uri(path);

        //    Uri relativeUri = baseUri.MakeRelativeUri(fullUri);

        //    // Uri's use forward slashes so convert back to backward slashes
        //    var result = Uri.UnescapeDataString(relativeUri.ToString());
        //    return result;

        //}

    }
}


