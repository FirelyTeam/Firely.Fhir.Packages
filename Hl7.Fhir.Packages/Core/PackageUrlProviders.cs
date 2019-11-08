﻿namespace Hl7.Fhir.Packages
{ 

    public enum PublishMode
    {
        New,
        Existing,
        Any
    }

    public class NodePackageUrlProvider : IPackageUrlProvider
    {
        public string Root { get; private set; } 

        public NodePackageUrlProvider(string root = null)
        {
            this.Root = root?.TrimEnd('/') ?? "https://registry.npmjs.org";
        }

        public string GetPackageListingUrl(string scope, string name) => $"{Root}/@{scope}%2F{name}";

        public string GetPackageListingUrl(string name) => $"{Root}/{name}";
            

        public string GetPackageUrl(PackageReference reference)
        {
            return
                (reference.Scope == null)
                ? $"{Root}/{reference.Name}/-/{reference.Name}-{reference.Version}.tgz"
                : $"{Root}/@{reference.Scope}/{reference.Name}/-/{reference.Name}-{reference.Version}.tgz";
        }

        public string GetPublishUrl(int fhirVersion, PackageReference reference, PublishMode mode)
        { 
            return $"{Root}/r{fhirVersion}/{reference.Name}";
        }

        public override string ToString() => $"(NPM) {Root}";
        
    }

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

        public string GetPublishUrl(int fhirVersion, PackageReference reference, PublishMode mode)
        {
            string url = $"{Root}/r{fhirVersion}?publishMode={mode}";
            return url;
        }
    }

    public static class PackageUrlProvider
    {
        public static IPackageUrlProvider Npm => new NodePackageUrlProvider("https://registry.npmjs.org");
        public static IPackageUrlProvider Simplifier => new FhirPackageUrlProvider("https://packages.simplifier.net");
        public static IPackageUrlProvider SimplifierNpm => new NodePackageUrlProvider("https://packages.simplifier.net");
        public static IPackageUrlProvider Staging => new FhirPackageUrlProvider("https://packages-staging.simplifier.net");
        public static IPackageUrlProvider StagingNpm => new NodePackageUrlProvider("https://packages-staging.simplifier.net");
        public static IPackageUrlProvider Localhost => new FhirPackageUrlProvider("http://packages.simplifier.ro/");

    }



}
