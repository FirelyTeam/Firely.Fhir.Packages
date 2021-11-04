﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages.Tests
{
    internal class TestHelper
    {


        /// <summary>
        /// Open and restore an NPM package from the given location.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="progressHandler"></param>
        /// <returns></returns>
        internal async static Task<PackageContext> Open(string path, Action<string> progressHandler)
        {
            var client = PackageClient.Create();
            var cache = new DiskPackageCache();
            var project = new FolderProject(path);

            var scope = new PackageContext(cache, project, client, progressHandler);

            var closure = await scope.Restore();

            if (closure.Missing.Any())
            {
                var missingDeps = String.Join(", ", closure.Missing);
                throw new FileNotFoundException($"Could not resolve all dependencies. Missing: {missingDeps}.");
            }

            return scope;
        }

        /// <summary>
        /// Create an NPM metapackage at a temporary location.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dependencies"></param>
        /// <returns></returns>
        internal async static Task<string> InitializeTemporary(string name, params string[] dependencies)
        {
            var tempDirectoryPart = $"packages-lib-tests-{name}-{Guid.NewGuid()}";
            var projectDirectory = Path.Combine(Path.GetTempPath(), tempDirectoryPart);

            _ = await Initialize(projectDirectory, name, "0.1.0", "Unit test", "Temporary metapackage",
                    dependencies);

            return projectDirectory;
        }

        /// <summary>
        /// Initialize an empty NPM metapackage at a given location. 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <param name="author"></param>
        /// <param name="description"></param>
        /// <param name="dependencies"></param>
        /// <returns></returns>
        /// <remarks>If there is already a package at the location, the information passed in the parameters will override what was already present.</remarks>
        private async static Task<PackageManifest> Initialize(string path, string? name = null, string? version = null,
            string? author = null, string? description = null, string[]? dependencies = null)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            var project = new FolderProject(path);

            var manifest = await project.ReadManifest() ?? createManifest(name);
            manifest.Author = author ?? manifest.Author ?? "supply your name here";
            manifest.Version = version ?? manifest.Version ?? "0.1.0";
            manifest.Description = description ?? manifest.Description ?? "describe your package";

            if (dependencies?.Any() == true)
            {
                var packageDependencies = createDependencies(dependencies);
                foreach (var packageDep in packageDependencies) manifest.AddDependency(packageDep);
            }

            await project.WriteManifest(manifest);
            return manifest;

            static PackageManifest createManifest(string? name)
            {
                var defaultName = $"name-your-package-{Guid.NewGuid()}";
                return new PackageManifest
                {
                    Name = name ?? defaultName
                };
            }

            static IEnumerable<PackageDependency> createDependencies(string[] dependencies)
            {
                foreach (var dep in dependencies)
                {
                    var splitDependency = dep.Split('@');
                    if (splitDependency.Length == 1)
                        yield return new PackageDependency(dep, "latest");
                    else
                    {
                        var versionDep = "=" + splitDependency[1];
                        yield return new PackageDependency(splitDependency[0], versionDep);
                    }
                }
            }
        }
    }
}
