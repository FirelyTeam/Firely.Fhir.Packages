using System.IO;

namespace Hl7.Fhir.Packages
{
    public static class Disk
    {
        public static string GetFolderName(string path)
        {
            return new DirectoryInfo(path).Name;
        }
        
     }
}


