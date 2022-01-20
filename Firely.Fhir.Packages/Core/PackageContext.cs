#nullable enable

using Hl7.Fhir.Utility;
using System;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{

    /// <summary>
    /// A PackageContext gives access to the full scope of a project or package.
    /// It also provides a single FileIndex to all files in scope, including
    /// the full dependency closure.
    /// </summary>
    public class PackageContext
    {
        public readonly IPackageCache Cache;
        public readonly IProject Project;
        public readonly IPackageServer Server;
        internal readonly Action<PackageReference>? onInstalled;

        private FileIndex? _index;

        /// <summary>
        /// Creates a package context. 
        /// </summary>
        /// <param name="cache">The cache from where to reference packages and consume dependency resources</param>
        /// <param name="project">The project or main package at the root of a dependency tree</param>
        /// <param name="server">The server from where to install packages into the cache</param>
        /// <param name="onInstalled">Event that responds to succesfull package installs</param>
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

        /// <summary>
        /// Read's the the full FileIndex from the current scope. This includes all resource files from all dependencies.
        /// </summary>
        public FileIndex GetIndex()
        {
            return _index ??= TaskHelper.Await(() => buildIndex());
        }

        private async Task<FileIndex> buildIndex()
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