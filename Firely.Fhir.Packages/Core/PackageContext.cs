#nullable enable

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

        public FileIndex Index => _index ??= BuildIndex().Result; // You cannot have async getters in C#, maybe not make this a property ?! (Paul)
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