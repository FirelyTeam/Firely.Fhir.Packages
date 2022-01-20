using Hl7.Fhir.Specification;

namespace Firely.Fhir.Packages
{

    public interface IPackageUrlProvider
    {
        /// <summary>
        /// Package root path
        /// </summary>
        string Root { get; }


        /// <summary>
        /// Get the Package listings url
        /// </summary>
        /// <param name="name">Package name</param>
        /// <returns>the package listings url</returns>
        string GetPackageListingUrl(string name);

        /// <summary>
        /// Get the version specific package url
        /// </summary>
        /// <param name="reference">Package reference</param>
        /// <returns>Package url</returns>
        string GetPackageUrl(PackageReference reference);

        /// <summary>
        /// Get the publish URL
        /// </summary>
        /// <param name="release">FHIR release version</param>
        /// <param name="reference">Package url</param>
        /// <param name="mode">Publish mode</param>
        /// <returns>A URL used for publishing packages</returns>
        string GetPublishUrl(FhirRelease release, PackageReference reference, PublishMode mode);
    }

}
