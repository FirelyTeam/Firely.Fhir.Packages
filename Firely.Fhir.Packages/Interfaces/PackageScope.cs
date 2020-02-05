using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{

    public class PackageScope 
    {
        public readonly IPackageCache Cache;
        public readonly IPackageIndex Server;
        public readonly IProject Project;
        internal readonly PackageClient Client;
        internal readonly PackageClosure closure;
        
        internal FileIndex Index => _index ??= BuildIndex();
      

        public PackageScope(IPackageCache cache, IProject project, PackageClient client = null)
        {
            this.Cache = cache;
            this.Server = cache; // for now
            this.Project = project;
            this.Client = client;
        }

        public PackageClosure LoadClosure()
        {
            Project.ReadClosure();
            if (closure is null) throw new ArgumentException("The folder does not contain a package lock file.");
            return closure;
        }

        private FileIndex BuildIndex()
        {
            var index = new FileIndex();
            var closure = Project.ReadClosure();
            Index.Index(Project);
            Index.Index(Cache, closure);
            return index;
        }

        private FileIndex _index;
    }




    public static class PackageScopeExtensions
    {
        public static bool TryResolveCanonical(this PackageScope scope, string uri, out string content)
        {
            var reference = scope.Index.ResolveCanonical(uri);
            if (reference is object)
            {
                content = scope.GetFileContent(reference);
                return true;
            }
            else
            { 
                content = null;
                return false;
            }
        }

        public static async Task<PackageReference> InstallPackage(this PackageScope scope, PackageDependency dependency)
        {
            var reference = await scope.Cache.Install(dependency);
            
            if (reference.Found)
            {
                scope.Project.Install(dependency);
            }
            
            return reference;
        }

        public static async Task<PackageReference> InstallPackage(this PackageScope scope, string name, string range)
        {
            var dep = new PackageDependency(name, range);
            return await scope.InstallPackage(dep);
        }

        public static PackageFileReference ResolveCanonical(this PackageScope scope, string canonical)
        {
            //var reference = scope.Project.ResolveCanonical(uri);
            return scope.Index.ResolveCanonical(canonical);
        }
        public static string GetFileContent(this PackageScope scope, PackageFileReference reference)
        {
            if (!reference.Package.Found) // this is a hack, because we cannot reference the project itself with a PackageReference
            {
                return scope.Project.GetFileContent(reference.FileName);
            }
            else
            {
                return scope.Cache.GetFileContent(reference);

            }
        }
    }
}
