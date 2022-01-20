#nullable enable

using System.IO;

namespace Firely.Fhir.Packages
{
    /// <summary>
    /// A package file entry
    /// </summary>
    public class FileEntry
    {
        /// <summary>      
        /// Initiate a <see cref="FileEntry"/> instance      
        /// </summary>
        /// <param name="filePath">the file path of the file entry</param>
        /// <param name="buffer">the content of the entry</param>
        public FileEntry(string filePath, byte[] buffer)
        {
            FilePath = filePath;
            Buffer = buffer;
        }

        /// <summary>
        /// the file path of the file entry
        /// </summary>
        public string FilePath;
        /// <summary>
        /// the file content of the file entry
        /// </summary>
        public byte[] Buffer;

        /// <summary>
        /// Gets a stream of the content of the file entry
        /// </summary>
        /// <returns></returns>
        public Stream GetStream()
        {
            return new MemoryStream(Buffer);
        }

        /// <summary>
        /// Gets the filename of the package
        /// </summary>
        public string FileName => Path.GetFileName(FilePath);
    }
}

#nullable restore

