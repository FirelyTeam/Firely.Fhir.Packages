using System.IO;

namespace Hl7.Fhir.Packages
{
    public class FileEntry
    {
        public string FilePath;
        public byte[] Buffer;

        public Stream GetStream()
        {
            return new MemoryStream(Buffer);
        }
    }

}


