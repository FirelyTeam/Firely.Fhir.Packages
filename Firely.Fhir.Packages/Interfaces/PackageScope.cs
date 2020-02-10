using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{

    public class PackageScope 
    {
        public readonly IPackageCache Cache;
        public readonly IProject Project;
        public readonly IPackageServer Server;
        internal PackageClosure closure;
        internal readonly Action<string> Report;

        internal FileIndex Index => _index ??= BuildIndex();

        public PackageScope(IPackageCache cache, IProject project, IPackageServer server, Action<string> report = null)
        {
            this.Cache = cache;
            this.Project = project;
            this.Server = server;
            this.Report = report;
        }

        private void LoadClosure()
        {
            closure = Project.ReadClosure();
            if (closure is null) throw new ArgumentException("The folder does not contain a package lock file.");
        }

        public FileIndex BuildIndex()
        {
            var index = new FileIndex();
            LoadClosure();
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

        public static async Task<PackageReference> Install(this PackageScope scope, string name, string range)
        {
            var dependency = new PackageDependency(name, range);
            return await scope.Install(dependency);
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
