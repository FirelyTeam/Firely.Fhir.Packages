using Hl7.Fhir.Specification;

namespace Firely.Fhir.Packages
{

    public interface IPackageUrlProvider
    {
        string Root { get; }
        string GetPackageListingUrl(string name);
        string GetPackageUrl(PackageReference reference);
        string GetPublishUrl(PackageReference reference, PublishMode mode);
    }

}
