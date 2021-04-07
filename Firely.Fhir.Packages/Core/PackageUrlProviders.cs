namespace Firely.Fhir.Packages
{

    public enum PublishMode
    {
        New,
        Existing,
        Any
    }

    public static class PackageUrlProviders
    {
        public static IPackageUrlProvider Npm => new NpmPackageUrlProvider("https://registry.npmjs.org");
        public static IPackageUrlProvider Simplifier => new FhirPackageUrlProvider("https://packages.simplifier.net");
        public static IPackageUrlProvider SimplifierNpm => new NpmPackageUrlProvider("https://packages.simplifier.net");
        public static IPackageUrlProvider Staging => new FhirPackageUrlProvider("https://packages-staging.simplifier.net");
        public static IPackageUrlProvider StagingNpm => new NpmPackageUrlProvider("https://packages-staging.simplifier.net");
        public static IPackageUrlProvider Localhost => new FhirPackageUrlProvider("http://packages.simplifier.ro/");

    }



}
