using Hl7.Fhir.Specification;

namespace Firely.Fhir.Packages
{
    public class FhirPackageUrlProvider : IPackageUrlProvider
    {
        public string Root { get; private set; }

        public FhirPackageUrlProvider(string root)
        {
            this.Root = root.TrimEnd('/');
        }

        public string GetPackageListingUrl(string name) => $"{Root}/{name}";

        public string GetPackageUrl(PackageReference reference) => $"{Root}/{reference.Name}/{reference.Version}";

        public override string ToString() => $"(FHIR) {Root}";

        public string GetPublishUrl(FhirRelease release, PackageReference reference, PublishMode mode)
        {
            string url = $"{Root}/{release}?publishMode={mode}";
            return url;
        }
    }



}
