﻿using System;
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

        public FileIndex Index => _index ??= BuildIndex().Result; // You cannot have async getters in C#, maybe not make this a property ?! (Paul)

        public PackageScope(IPackageCache cache, IProject project, IPackageServer server, Action<string> report = null)
        {
            this.Cache = cache;
            this.Project = project;
            this.Server = server;
            this.Report = report;
        }

        private async Task<PackageClosure> ReadClosure()
        {
            closure = await Project.ReadClosure();
            if (closure is null) throw new ArgumentException("The folder does not contain a package lock file.");
            return closure;
        }

        public async Task<FileIndex> BuildIndex()
        {
            this.closure = await ReadClosure();

            var index = new FileIndex();
            await index.Index(Project);
            await index.Index(Cache, closure);

            return index;
        }

        private FileIndex _index;
    }

    public static class PackageScopeExtensions
    {
        public static async Task<string> GetFileContentByCanonical(this PackageScope scope, string uri)
        {
            var reference = scope.Index.ResolveCanonical(uri);

            if (reference is object)
            {
                return await scope.GetFileContent(reference);               
            }
            else
            {
                return null;
            }
        }

        public static async Task<PackageReference> Install(this PackageScope scope, string name, string range)
        {
            var dependency = new PackageDependency(name, range);

            return await scope.Install(dependency);
        }

        public static PackageFileReference GetFileReferenceByCanonical(this PackageScope scope, string canonical)
        {
            return scope.Index.ResolveCanonical(canonical);
        }

        public static async Task<string> GetFileContent(this PackageScope scope, PackageFileReference reference)
        {
            if (!reference.Package.Found) // this is a hack, because we cannot reference the project itself with a PackageReference
            {
                return await scope.Project.GetFileContent(reference.FileName);
            }
            else
            {
                return await scope.Cache.GetFileContent(reference);

            }
        }
    }
}
