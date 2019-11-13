﻿using System.Threading.Tasks;

namespace Hl7.Fhir.Packages
{
    public interface IPackageInstaller
    {
        ValueTask<PackageReference> ResolveDependency(PackageDependency dependency);
        ValueTask<PackageManifest> InstallPackage(PackageReference reference);
        ValueTask<bool> IsInstalled(PackageDependency dependency);
    }

   

}
