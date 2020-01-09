namespace Hl7.Fhir.Packages
{
    public static class PackageFactory
    {
        public static IPackageCache GlobalPackageCache()
        {
            return new DiskPackageCache(Platform.GetFhirPackageRoot());
        }
    }


}


