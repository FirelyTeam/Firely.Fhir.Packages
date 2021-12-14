#nullable enable

using Hl7.Fhir.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public static class PackageContextExtensions
    {
        public static async Task<string?> GetFileContentByCanonical(this PackageContext scope, string uri, string? version = null, bool resolveBestCandidate = false)
        {
            var reference = resolveBestCandidate
                ? scope.Index.ResolveBestCandidateByCanonical(uri, version)
                : scope.Index.ResolveCanonical(uri, version);

            return reference is not null ? await scope.getFileContent(reference).ConfigureAwait(false) : null;
        }

        public static async Task<PackageReference> Install(this PackageContext scope, string name, string range)
        {
            var dependency = new PackageDependency(name, range);
            return await scope.CacheInstall(dependency).ConfigureAwait(false);
        }

        public static PackageFileReference? GetFileReferenceByCanonical(this PackageContext scope, string uri, string? version = null, bool resolveBestCandidate = false)
        {
            return resolveBestCandidate
                ? scope.Index.ResolveBestCandidateByCanonical(uri, version)
                : scope.Index.ResolveCanonical(uri, version);
        }

        private static async Task<string> getFileContent(this PackageContext scope, PackageFileReference reference)
        {
            return !reference.Package.Found
                ? await scope.Project.GetFileContent(reference.FilePath).ConfigureAwait(false)
                : await scope.Cache.GetFileContent(reference).ConfigureAwait(false);
        }

        public static IEnumerable<string> ReadAllFiles(this PackageContext scope)
        {
            foreach (var reference in scope.Index)
            {
                var content = TaskHelper.Await(() => scope.getFileContent(reference));
                yield return content;
            }
        }

        public static IEnumerable<string> GetContentsForRange(this PackageContext scope, IEnumerable<PackageFileReference> references)
        {
            foreach (var item in references)
            {
                var content = TaskHelper.Await(() => scope.getFileContent(item));
                yield return content;
            }
        }

        public static async Task EnsureManifest(this PackageContext scope, string name, string fhirVersion)
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

        public static async Task<InstallResult> Install(this PackageContext scope, PackageDependency dependency)
        {
            var reference = await scope.CacheInstall(dependency);
            if (reference.NotFound) throw new Exception($"Package '{dependency}' was not found.");

            if (!await scope.Project.HasManifest())
            {
                var fhirVersion = await scope.Cache.ReadPackageFhirVersion(reference);
                await scope.EnsureManifest("project", fhirVersion).ConfigureAwait(false);
            }

            await scope.Project.AddDependency(dependency).ConfigureAwait(false);

            var closure = await scope.Restore().ConfigureAwait(false);
            return new InstallResult(closure, reference);
        }

        private static PackageFileReference? getFileReference(this PackageContext scope, string resourceType, string id)
        {
            return scope.Index.Where(i => i.ResourceType == resourceType && i.Id == id).FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="resourceType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<string?> GetFileContentById(this PackageContext scope, string resourceType, string id)
        {
            var reference = scope.getFileReference(resourceType, id);
            return reference is null ? null : await scope.getFileContent(reference).ConfigureAwait(false);
        }

        public static IEnumerable<string> GetFileNames(this PackageContext scope)
        {
            return scope.Index.Select(i => i.FileName);
        }

        /// <summary>
        /// Returns the content of a file, represented as string by filepath
        /// </summary>
        /// <param name="scope">The package containing the resources</param>
        /// <param name="filePath">Filepath of the content to be returned</param>
        /// <returns>the content of a file, represented as string by filepath</returns>
        public static async Task<string?> GetFileContentByFileName(this PackageContext scope, string fileName)
        {
            var reference = scope.Index.Where(i => i.FileName == fileName).FirstOrDefault();
            if (reference is null) return null;

            var content = await scope.getFileContent(reference).ConfigureAwait(false);
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
            var reference = scope.Index.Where(i => i.FilePath == filePath).FirstOrDefault();
            if (reference is null) return null;

            var content = await scope.getFileContent(reference).ConfigureAwait(false);
            return content;
        }

        /// <summary>
        /// Lists all canonical Uri of a package, with optional filter on resource type 
        /// </summary>
        /// <param name="scope">The package containing the resources</param>
        /// <param name="resourceType">Resource type as string used to filter</param>
        /// <returns>Sequence of uri strings.</returns>
        public static IEnumerable<string> ListCanonicalUris(this PackageContext scope, string? resourceType = null)
        {
            return (resourceType is not null)
                ? scope.Index.Where(i => i.ResourceType == resourceType && i.Canonical is not null).Select(i => i.Canonical!)
                : scope.Index.Where(i => i.Canonical is not null).Select(i => i.Canonical!);
        }


        /// <summary>
        /// Find a CodeSystem resource by a ValueSet canonical url that contains all codes from that codesystem.
        /// </summary>
        /// <param name="scope">The package context in which to find the CodeSystem and ValueSet</param>
        /// <param name="valueSetUri">The canonical uri of a ValueSet resource.</param>
        /// <returns>A CodeSystem resource, or null.</returns>
        /// <remarks>
        /// It is very common for valuesets to represent all codes from a specific/smaller code system.
        /// These are indicated by he CodeSystem.valueSet element, which is searched here.
        /// </remarks>
        public static async Task<string?> GetCodeSystemByValueSet(this PackageContext scope, string valueSetUri)
        {
            var vsReference = scope.Index.Where(i => i.Canonical == valueSetUri).FirstOrDefault();
            if (vsReference?.ValueSetCodeSystem is null) return null;
            else
            {
                (var uri, var version) = vsReference.ValueSetCodeSystem.Splice('|');

                return uri is not null ? await scope.GetFileContentByCanonical(uri, version).ConfigureAwait(false) : null;
            }
        }

        /// <summary>Find ConceptMap resources which map from the given source to the given target.</summary>
        /// <param name="scope">The package context in which to find the ConceptMap</param>
        /// <param name="sourceUri">An uri that is either the source uri, source ValueSet system or source StructureDefinition canonical url for the map.</param>
        /// <param name="targetUri">An uri that is either the target uri, target ValueSet system or target StructureDefinition canonical url for the map.</param>
        /// <returns>A sequence of ConceptMap resources.</returns>
        /// <remarks>Either sourceUri may be null, or targetUri, but not both</remarks>
        public static async Task<IEnumerable<string>?> GetConceptMapsBySourceAndTarget(this PackageContext scope, string? sourceUri, string? targetUri)
        {
            if (sourceUri == null && targetUri == null)
                return null;

            var conceptMapReferences = scope.Index.Where(i => i.ConceptMapUris?.SourceUri == sourceUri && i.ConceptMapUris?.TargetUri == targetUri);
            return await Task.WhenAll(conceptMapReferences.Select(async i => await scope.getFileContent(i).ConfigureAwait(false))).ConfigureAwait(false);
        }

        /// <summary>Finds a NamingSystem resource by matching any of a system's UniqueIds.</summary>
        ///<param name="scope">The package context in which to find the NamingSystem</param>
        /// <param name="uniqueId">The unique id of a NamingSystem resource.</param>
        /// <returns>A NamingSystem resource, or <c>null</c>.</returns>
        public static async Task<string?> GetNamingSystemByUniqueId(this PackageContext scope, string uniqueId)
        {
            var namingSystemIndex = scope.Index.Where(i => i.NamingSystemUniqueId?.Contains(uniqueId) == true).FirstOrDefault();

            return namingSystemIndex is not null ? await scope.getFileContent(namingSystemIndex).ConfigureAwait(false) : null;
        }
    }
}

#nullable restore