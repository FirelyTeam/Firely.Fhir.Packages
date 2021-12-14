#nullable enable

namespace Firely.Fhir.Packages
{
    /// <summary>
    /// A reference to a file in a package, or pseudo package.
    /// </summary>
    public class PackageFileReference : ResourceMetadata
    {
        public PackageFileReference(string filename, string filepath) : base(filename, filepath)
        {
            FileName = filename;
            FilePath = filepath;
        }

        public PackageReference Package;
    }

}

#nullable restore