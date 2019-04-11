namespace Hl7.Fhir.Packages
{
    public interface IPackageUrlProvider
    {
        string Root { get; }
        string GetPackageListingUrl(string name);
        string GetPackageUrl(PackageReference reference);
    }

}
