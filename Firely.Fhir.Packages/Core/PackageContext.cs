#nullable enable

using Hl7.Fhir.Utility;
using System;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{

    public class PackageContext
    {
        public readonly IPackageCache Cache;
        public readonly IProject Project;
        public readonly IPackageServer Server;
        internal readonly Action<PackageReference>? onInstalled;

        private FileIndex? _index;

        public PackageContext(IPackageCache cache, IProject project, IPackageServer server, Action<PackageReference>? onInstalled = null)
        {
            this.Cache = cache;
            this.Project = project;
            this.Server = server;
            this.onInstalled = onInstalled;
        }

        private async Task<PackageClosure> readClosure()
        {
            var closure = await Project.ReadClosure();
            return closure is null ? throw new ArgumentException("The folder does not contain a package lock file.") : closure;
        }

        //MS 2021-12-15:  This used to be a variable, but you can't have async variables in .NET, changed to a getter
        public FileIndex GetIndex()
        {
            return _index ??= TaskHelper.Await(() => BuildIndex());
        }

        public async Task<FileIndex> BuildIndex()
        {
            var closure = await readClosure();

            var index = new FileIndex();
            await index.Index(Project);
            await index.Index(Cache, closure);

            return index;
        }
    }
}

#nullable restore