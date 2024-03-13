/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


#nullable enable

using Hl7.Fhir.Model;
using Hl7.Fhir.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public static class PackageContextExtensions
    {
        /// <summary>
        /// Retrieve file content from a package by canonical uri
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="uri">Canonical uri of the resource</param>
        /// <param name="version">Version of the conformance resource</param>
        /// <param name="resolveBestCandidate">If there are multiple candidates, try to resolve the best instead of the first</param>
        /// <param name="fhirVersion">Only resolve files that conform to this FHIR version</param>
        /// <returns>File content of the conformance resource</returns>
        public static async Task<string?> GetFileContentByCanonical(this PackageContext scope, string uri, string? version = null, bool resolveBestCandidate = false, FHIRVersion? fhirVersion = null)
        {
            var reference = resolveBestCandidate
                ? scope.GetIndex().ResolveBestCandidateByCanonical(uri, version, fhirVersion)
                : scope.GetIndex().ResolveCanonical(uri, version, fhirVersion);

            return reference is not null ? await scope.GetFileContent(reference).ConfigureAwait(false) : null;
        }

        /// <summary>
        /// Install the package
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="name">Name of the package</param>
        /// <param name="range">Version range</param>
        /// <returns>Paxckage reference of the installed package</returns>
        public static async Task<PackageReference> Install(this PackageContext scope, string name, string range)
        {
            var dependency = new PackageDependency(name, range);
            return await scope.CacheInstall(dependency).ConfigureAwait(false);
        }
        /// <summary>
        /// Retrieve file content from a package by canonical uri
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="uri">Canonical uri of the resource</param>
        /// <param name="version">Version of the conformance resource</param>
        /// <param name="resolveBestCandidate">If there are multiple candidates, try to resolve the best instead of the first</param>
        /// <returns>File content of the conformance resource</returns>
        public static PackageFileReference? GetFileReferenceByCanonical(this PackageContext scope, string uri, string? version = null, bool resolveBestCandidate = false)
        {
            return resolveBestCandidate
                ? scope.GetIndex().ResolveBestCandidateByCanonical(uri, version)
                : scope.GetIndex().ResolveCanonical(uri, version);
        }

        /// <summary>
        /// Retrieve the content of a specific file in a package
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="reference">File reference that represents the file</param>
        /// <returns>Content of the file</returns>
        public static async Task<string> GetFileContent(this PackageContext scope, PackageFileReference reference)
        {
            return !reference.Package.Found
                ? await scope.Project.GetFileContent(reference.FilePath).ConfigureAwait(false)
                : await scope.Cache.GetFileContent(reference).ConfigureAwait(false);
        }

        /// <summary>
        /// Read all files from a package
        /// </summary>
        /// <param name="scope"></param>
        /// <returns>List of file content, represented as string</returns>
        public static IEnumerable<string> ReadAllFiles(this PackageContext scope)
        {
            foreach (var reference in scope.GetIndex())
            {
                var content = TaskHelper.Await(() => scope.GetFileContent(reference));
                yield return content;
            }
        }

        /// <summary>
        /// Get content for multiple files
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="references">File references representing multiple files</param>
        /// <returns>A list of file contents</returns>
        public static IEnumerable<string> GetContentsForRange(this PackageContext scope, IEnumerable<PackageFileReference> references)
        {
            foreach (var item in references)
            {
                var content = TaskHelper.Await(() => scope.GetFileContent(item));
                yield return content;
            }
        }

        private static async Task ensureManifest(this PackageContext scope, string name, string fhirVersion)
        {
            var manifest = await scope.Project.ReadManifest();
            manifest ??= ManifestFile.Create(name, fhirVersion);
            await scope.Project.WriteManifest(manifest).ConfigureAwait(false);
        }



        public class InstallResult
        {
            public InstallResult(PackageClosure closure, PackageReference reference)
            {
                Closure = closure;
                Reference = reference;
            }

            public PackageClosure Closure;
            public PackageReference Reference;
        }

        /// <summary>
        /// Installs a package
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="dependency">Package dependendies</param>
        /// <returns>Install result</returns>
        /// <exception cref="Exception">Exception thrown when a dependency can't be found</exception>
        public static async Task<InstallResult> Install(this PackageContext scope, PackageDependency dependency)
        {
            var reference = await scope.CacheInstall(dependency);
            if (reference.NotFound) throw new Exception($"Package '{dependency}' was not found.");

            if (!await scope.Project.HasManifest())
            {
                var fhirVersion = await scope.Cache.ReadPackageFhirVersion(reference);
                await scope.ensureManifest("project", fhirVersion).ConfigureAwait(false);
            }

            await scope.Project.AddDependency(dependency).ConfigureAwait(false);

            var closure = await scope.Restore().ConfigureAwait(false);
            return new InstallResult(closure, reference);
        }

        private static PackageFileReference? getFileReference(this PackageContext scope, string resourceType, string id)
        {
            return scope.GetIndex().Where(i => i.ResourceType == resourceType && i.Id == id).FirstOrDefault();
        }

        /// <summary>
        /// Get file content by resource type and Id
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="resourceType">The FHIR resource type of the file</param>
        /// <param name="id">The id of the FHIR resource</param>
        /// <returns>The file content represented as string</returns>
        public static async Task<string?> GetFileContentById(this PackageContext scope, string resourceType, string id)
        {
            var reference = scope.getFileReference(resourceType, id);
            return reference is null ? null : await scope.GetFileContent(reference).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the list of filenames from a Package
        /// </summary>
        /// <param name="scope"></param>
        /// <returns>List of file names</returns>
        public static IEnumerable<string> GetFileNames(this PackageContext scope)
        {
            return scope.GetIndex().Select(i => i.FileName);
        }

        /// <summary>
        /// Returns the content of a file, represented as string by filepath
        /// </summary>
        /// <param name="scope">The package containing the resources</param>
        /// <param name="filePath">Filepath of the content to be returned</param>
        /// <returns>the content of a file, represented as string by filepath</returns>
        public static async Task<string?> GetFileContentByFileName(this PackageContext scope, string fileName)
        {
            var reference = scope.GetIndex().Where(i => i.FileName == fileName).FirstOrDefault();
            if (reference is null) return null;

            var content = await scope.GetFileContent(reference).ConfigureAwait(false);
            return content;
        }

        /// <summary>
        /// Returns the content of a file, represented as string by filepath
        /// </summary>
        /// <param name="scope">The package containing the resources</param>
        /// <param name="filePath">Filepath of the content to be returned</param>
        /// <returns>the content of a file, represented as string by filepath</returns>
        public static async Task<string?> GetFileContentByFilePath(this PackageContext scope, string filePath)
        {
            var reference = scope.GetIndex().Where(i => i.FilePath == filePath).FirstOrDefault();
            if (reference is null) return null;

            var content = await scope.GetFileContent(reference).ConfigureAwait(false);
            return content;
        }

        /// <summary>
        /// Lists all canonical Uri's of a package, with optional filter on resource type 
        /// </summary>
        /// <param name="scope">The package containing the resources</param>
        /// <param name="resourceType">Resource type as string used to filter</param>
        /// <returns>Sequence of canonical uri strings.</returns>
        public static IEnumerable<string> ListCanonicalUris(this PackageContext scope, string? resourceType = null)
        {
            return (resourceType is not null)
                ? scope.GetIndex().Where(i => i.ResourceType == resourceType && i.Canonical is not null).Select(i => i.Canonical!)
                : scope.GetIndex().Where(i => i.Canonical is not null).Select(i => i.Canonical!);
        }

        /// <summary>
        /// Lists all resource Uri's of a package (not canonical Uris), with optional filter on resource type 
        /// </summary>
        /// <param name="scope">The package containing the resources</param>
        /// <param name="resourceType">Resource type as string used to filter</param>
        /// <returns>Sequence of uri strings.</returns>
        public static IEnumerable<string> ListResourceUris(this PackageContext scope, string? resourceType = null)
        {
            return (resourceType is not null)
                ? scope.GetIndex().Where(i => i.ResourceType == resourceType && i.Id != null).Select(i => $"{i.ResourceType}/{i.Id}")
                : scope.GetIndex().Where(i => i.ResourceType is not null && i.Id != null).Select(i => $"{i.ResourceType}/{i.Id}");
        }



        /// <summary>
        /// Find a CodeSystem resource by a ValueSet canonical url that contains all codes from that codesystem.
        /// </summary>
        /// <param name="scope">The package context in which to find the CodeSystem</param>
        /// <param name="valueSetUri">The canonical uri of a ValueSet resource.</param>
        /// <returns>A CodeSystem resource, or null.</returns>
        /// <remarks>
        /// It is very common for valuesets to represent all codes from a specific/smaller code system.
        /// These are indicated by the CodeSystem.valueSet element, which is searched here.
        /// </remarks>
        public static async Task<string?> GetCodeSystemByValueSet(this PackageContext scope, string valueSetUri)
        {
            var codeSystem = scope.GetIndex().Where(i => i.ValueSetCodeSystem == valueSetUri).FirstOrDefault();
            return codeSystem?.Canonical is not null
                ? await scope.GetFileContentByCanonical(codeSystem.Canonical, codeSystem.Version).ConfigureAwait(false)
                : null;
        }

        /// <summary>Find ConceptMap resources which map from the given source to the given target.</summary>
        /// <param name="scope">The package context in which to find the ConceptMap</param>
        /// <param name="sourceUri">An uri that is either the source uri, source ValueSet system or source StructureDefinition canonical url for the map.</param>
        /// <param name="targetUri">An uri that is either the target uri, target ValueSet system or target StructureDefinition canonical url for the map.</param>
        /// <returns>A sequence of ConceptMap resources.</returns>
        /// <remarks>Either sourceUri may be null, or targetUri, but not both</remarks>
        public static async Task<IEnumerable<string>> GetConceptMapsBySourceAndTarget(this PackageContext scope, string? sourceUri, string? targetUri)
        {
            var conceptMapReferences = Enumerable.Empty<PackageFileReference>();

            if (sourceUri == null && targetUri == null)
                return Enumerable.Empty<string>();
            else if (sourceUri is not null && targetUri is null)
            {
                conceptMapReferences = scope.GetIndex().Where(i => i.ConceptMapUris?.SourceUri == sourceUri);
            }
            else if (sourceUri is null && targetUri is not null)
            {
                conceptMapReferences = scope.GetIndex().Where(i => i.ConceptMapUris?.TargetUri == targetUri);
            }
            else
            {
                conceptMapReferences = scope.GetIndex().Where(i => i.ConceptMapUris?.SourceUri == sourceUri && i.ConceptMapUris?.TargetUri == targetUri);
            }
            return await Task.WhenAll(conceptMapReferences.Select(async i => await scope.GetFileContent(i).ConfigureAwait(false))).ConfigureAwait(false);
        }

        /// <summary>Finds a NamingSystem resource by matching any of a system's UniqueIds.</summary>
        ///<param name="scope">The package context in which to find the NamingSystem</param>
        /// <param name="uniqueId">The unique id of a NamingSystem resource.</param>
        /// <returns>A NamingSystem resource, or <c>null</c>.</returns>
        public static async Task<string?> GetNamingSystemByUniqueId(this PackageContext scope, string uniqueId)
        {
            var namingSystemIndex = scope.GetIndex().Where(i => i.NamingSystemUniqueId?.Contains(uniqueId) == true).FirstOrDefault();

            return namingSystemIndex is not null ? await scope.GetFileContent(namingSystemIndex).ConfigureAwait(false) : null;
        }
    }
}

#nullable restore