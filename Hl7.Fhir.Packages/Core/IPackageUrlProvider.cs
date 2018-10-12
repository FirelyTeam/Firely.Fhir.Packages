namespace Hl7.Fhir.Packages
{
    public interface IPackageUrlProvider
    {
        string Root { get; }
        string GetPackageListingUrl(PackageReference reference);
        string GetPackageUrl(PackageReference reference);
    }

}
