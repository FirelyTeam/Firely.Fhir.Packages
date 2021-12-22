#nullable enable

using System.IO;

namespace Firely.Fhir.Packages
{
    public static class Disk
    {
        /// <summary>
        /// Get name of a folder
        /// </summary>
        /// <param name="path">the path of the folder</param>
        /// <returns>the name of the folder</returns>
        public static string GetFolderName(string path)
        {
            return new DirectoryInfo(path).Name;
        }

    }
}


#nullable restore