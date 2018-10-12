namespace Hl7.Fhir.Packages
{
    public static class PackageFactory
    {
        public static PackageCache GlobalPackageCache()
        {
            return new PackageCache(Platform.GetFhirPackageRoot());
        }
    }


}


