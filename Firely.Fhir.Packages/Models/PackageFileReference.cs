#nullable enable

namespace Firely.Fhir.Packages
{
    /// <summary>
    /// A reference to a file in a package, or pseudo package.
    /// </summary>
    public class PackageFileReference : ResourceMetadata
    {
        /// <summary>
        /// Initialized a new <see cref="PackageFileReference"/>
        /// </summary>
        /// <param name="filename">Name of the file</param>
        /// <param name="filepath">File path, relative to the package root</param>
        public PackageFileReference(string filename, string filepath) : base(filename, filepath)
        {
            FileName = filename;
            FilePath = filepath;
        }

        /// <summary>
        /// references to the package that this file belongs to
        /// </summary>
        public PackageReference Package;
    }

}

#nullable restore