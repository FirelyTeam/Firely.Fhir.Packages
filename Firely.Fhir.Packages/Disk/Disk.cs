#nullable enable

using System.IO;

namespace Firely.Fhir.Packages
{
    internal static class Disk
    {
        internal static string GetFolderName(string path)
        {
            return new DirectoryInfo(path).Name;
        }

    }
}


#nullable restore