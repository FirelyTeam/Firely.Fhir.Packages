#nullable enable

namespace Firely.Fhir.Packages;

using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Specification;
using Hl7.Fhir.Specification.Source;
using Hl7.Fhir.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>Reads FHIR artifacts (Profiles, ValueSets, ...) from one or multiple FHIR packages. This functionaly is FHIR version agnostic.</summary>
public class FhirPackageSource : IAsyncResourceResolver, IArtifactSource
{
    private Lazy<PackageContext> _context;
    private ModelInspector _provider;

    /// <summary>Create a new <see cref="FhirPackageSource"/> instance to read FHIR artifacts from one or multiple FHIR packages of a specific FHIR version
    /// found in the paths passed to this function.</summary>
    /// <returns>A new <see cref="FhirPackageSource"/> instance.</returns>
    /// <param name="provider">A <see cref="ModelInspector"/> used to parse the filecontents to FHIR resources, this is typically a <see cref="ModelInspector"/> containing the definitions of a specific FHIR version. </param>
    /// <param name="filePaths">A path to the FHIR package files.</param>
    public FhirPackageSource(ModelInspector provider, params string[] filePaths)
    {
        _context = new Lazy<PackageContext>(() => TaskHelper.Await(() => createPackageContextFromFilesAsync(filePaths)));
        _provider = provider;
    }

    /// <summary>Create a new <see cref="FhirPackageSource"/> instance to read FHIR artifacts from one or multiple FHIR packages of a specific FHIR version.</summary>
    /// <param name="provider">A <see cref="ModelInspector"/> used to parse the file contents to FHIR resources, this is typically a <see cref="ModelInspector"/> containing the definitions of a specific FHIR version. </param>
    /// <param name="packageServer">The package server from which to retrieve the FHIR packages</param>
    /// <param name="packageNames">The FHIR packages which are used to resolve artifacts from</param>
    public FhirPackageSource(ModelInspector provider, string packageServer, string[] packageNames)
    {
        _context = new Lazy<PackageContext>(() => TaskHelper.Await(() => createPackageContextFromExternalSource(packageServer, packageNames)));
        _provider = provider;
    }

    /// <summary>Create a new <see cref="FhirPackageSource"/> instance to read FHIR artifacts from one or multiple FHIR packages of a specific FHIR version,
    /// retrieved from a package server that requires authentication, such as a private Simplifier.net package feed
    /// (e.g. <c>https://packages.simplifier.net/feeds/{feedname}</c>).</summary>
    /// <param name="provider">A <see cref="ModelInspector"/> used to parse the file contents to FHIR resources, this is typically a <see cref="ModelInspector"/> containing the definitions of a specific FHIR version. </param>
    /// <param name="packageServer">The package server from which to retrieve the FHIR packages</param>
    /// <param name="packageNames">The FHIR packages which are used to resolve artifacts from</param>
    /// <param name="tokenProvider">A function that supplies the Bearer token used to authenticate against the package server.
    /// The function is invoked for every request, so it can supply a refreshed token when a previous one has expired.</param>
    public FhirPackageSource(ModelInspector provider, string packageServer, string[] packageNames, Func<Task<string>> tokenProvider)
    {
        _context = new Lazy<PackageContext>(() => TaskHelper.Await(() => createPackageContextFromExternalSource(packageServer, packageNames, tokenProvider)));
        _provider = provider;
    }

    // ReSharper disable MemberCanBePrivate.Global
    // List the "core" packages for each FHIR version, these are the packages that contain the base FHIR resources and definitions, and are typically used as dependencies for other packages. Terminology expansions, tools, and extensions packages are included as well.
    // The extensions and tools packages use "latest" by default; use the CreateCorePackageSource overload with explicit version parameters to pin specific versions.
    // Note that DSTU2 and R4B do not have extensions or tools packages.

    public static readonly string[] DSTU2_CORE_PACKAGES = ["hl7.fhir.r2.core@1.0.2", "hl7.fhir.r2.expansions@1.0.2"];
    public static readonly string[] STU3_CORE_PACKAGES = ["hl7.fhir.r3.core@3.0.2", "hl7.fhir.r3.expansions@3.0.2", "hl7.fhir.uv.extensions.r3@latest", "hl7.fhir.uv.tools.r3@latest"];
    public static readonly string[] R4_CORE_PACKAGES = ["hl7.fhir.r4.core@4.0.1", "hl7.fhir.r4.expansions@4.0.1", "hl7.fhir.uv.extensions.r4@latest", "hl7.fhir.uv.tools.r4@latest"];
    public static readonly string[] R4B_CORE_PACKAGES = ["hl7.fhir.r4b.core@4.3.0", "hl7.fhir.r4b.expansions@4.3.0"];
    public static readonly string[] R5_CORE_PACKAGES = ["hl7.fhir.r5.core@5.0.0", "hl7.fhir.r5.expansions@5.0.0", "hl7.fhir.uv.extensions.r5@latest", "hl7.fhir.uv.tools.r5@latest"];

    public const string DEFAULT_PACKAGE_SERVER = "https://packages.simplifier.net";
    // ReSharper restore MemberCanBePrivate.Global

    /// <summary>
    /// Initializes a FhirPackageSource with the core FHIR packages of a specific FHIR version found on a package server.
    /// Terminology expansions and tools and extensions packages are included as well.
    /// By default the latest available versions of the extensions and tools packages are used; pass <paramref name="extensionsVersion"/>
    /// and/or <paramref name="toolsVersion"/> to pin those to specific versions instead.
    /// </summary>
    /// <param name="provider">A <see cref="ModelInspector"/> used to parse the file contents to FHIR resources, this is typically a <see cref="ModelInspector"/> containing the definitions of a specific FHIR version. </param>
    /// <param name="version">The FHIR version for which the core packages should be retrieved, if not specified, the FHIR version of the provided <see cref="ModelInspector"/> will be used.</param>
    /// <param name="packageServer">The package server from which to retrieve the FHIR packages, if not specified, the Simplifier.net package server will be used.</param>
    /// <param name="extensionsVersion">The version of the FHIR extensions package (hl7.fhir.uv.extensions) to use. When <c>null</c>, the latest available version is used.</param>
    /// <param name="toolsVersion">The version of the FHIR tools package (hl7.fhir.uv.tools) to use. When <c>null</c>, the latest available version is used.</param>
    /// <exception cref="NotSupportedException">
    /// Thrown when the specified FHIR version is not supported. DSTU1 and R6 have no packages available.
    /// DSTU2 and R4B do not have extensions or tools packages and therefore do not support <paramref name="extensionsVersion"/> or <paramref name="toolsVersion"/>.
    /// </exception>
    public static FhirPackageSource CreateCorePackageSource(ModelInspector provider, FhirRelease? version = null, string? packageServer = null,
        string? extensionsVersion = null, string? toolsVersion = null)
    {
        version ??= provider.FhirRelease;
        packageServer ??= DEFAULT_PACKAGE_SERVER;

        // When no version pinning is requested, use the pre-built package lists (which default to @latest for extensions/tools).
        if (extensionsVersion is null && toolsVersion is null)
        {
            return version switch
            {
                FhirRelease.DSTU1 => throw new NotSupportedException($"There are no packages available for FHIR version DSTU1."),
                FhirRelease.DSTU2 => new FhirPackageSource(provider, packageServer, DSTU2_CORE_PACKAGES),
                FhirRelease.STU3 => new FhirPackageSource(provider, packageServer, STU3_CORE_PACKAGES),
                FhirRelease.R4 => new FhirPackageSource(provider, packageServer, R4_CORE_PACKAGES),
                FhirRelease.R4B => new FhirPackageSource(provider, packageServer, R4B_CORE_PACKAGES),
                FhirRelease.R5 => new FhirPackageSource(provider, packageServer, R5_CORE_PACKAGES),
                FhirRelease.R6 => throw new NotSupportedException($"There are no packages available for FHIR version R6 yet."),
                _ => throw new NotSupportedException($"{nameof(CreateCorePackageSource)} has no support yet for version '{version}', please report this to the developers.")
            };
        }

        // Otherwise, build the package list with the specified versions, falling back to @latest for whichever is unspecified.
        var packages = version switch
        {
            FhirRelease.DSTU1 => throw new NotSupportedException($"There are no packages available for FHIR version DSTU1."),
            FhirRelease.DSTU2 => throw new NotSupportedException($"FHIR version DSTU2 does not have extensions or tools packages."),
            FhirRelease.STU3 => buildCorePackagesWithVersions(["hl7.fhir.r3.core@3.0.2", "hl7.fhir.r3.expansions@3.0.2"], "r3", extensionsVersion ?? "latest", toolsVersion ?? "latest"),
            FhirRelease.R4 => buildCorePackagesWithVersions(["hl7.fhir.r4.core@4.0.1", "hl7.fhir.r4.expansions@4.0.1"], "r4", extensionsVersion ?? "latest", toolsVersion ?? "latest"),
            FhirRelease.R4B => throw new NotSupportedException($"FHIR version R4B does not have extensions or tools packages."),
            FhirRelease.R5 => buildCorePackagesWithVersions(["hl7.fhir.r5.core@5.0.0", "hl7.fhir.r5.expansions@5.0.0"], "r5", extensionsVersion ?? "latest", toolsVersion ?? "latest"),
            FhirRelease.R6 => throw new NotSupportedException($"There are no packages available for FHIR version R6 yet."),
            _ => throw new NotSupportedException($"{nameof(CreateCorePackageSource)} has no support yet for version '{version}', please report this to the developers.")
        };

        return new FhirPackageSource(provider, packageServer, packages);
    }

    private static string[] buildCorePackagesWithVersions(string[] basePackages, string fhirVersionLabel, string extensionsVersion, string toolsVersion)
    {
        return [.. basePackages,
            $"hl7.fhir.uv.extensions.{fhirVersionLabel}@{extensionsVersion}",
            $"hl7.fhir.uv.tools.{fhirVersionLabel}@{toolsVersion}"];
    }

    private static async Task<PackageContext> createPackageContextFromExternalSource(string packageServer, string[] packageNames, Func<Task<string>>? tokenProvider = null)
    {
        var client = tokenProvider is null
            ? PackageClient.Create(packageServer)
            : PackageClient.Create(packageServer, tokenProvider);
        var scopePath = getScopePath();
        _ = await initialize(scopePath, "Firely SDK Temp Package", "0.1.0", "Firely SDK", "Temporary package used for resolving artifacts from its dependencies", packageNames);
        return await createContext(scopePath, client);

    }

    private static async Task<PackageManifest> initialize(string path, string name, string version, string author, string description, string[] dependencies)
    {
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        var project = new FolderProject(path);

        var manifest = new PackageManifest(name, version)
        {
            Author = author,
            Description = description,
        };

        if (dependencies?.Any() == true)
        {
            var packageDependencies = createDependencies(dependencies);
            foreach (var packageDep in packageDependencies) manifest.AddDependency(packageDep);
        }

        await project.WriteManifest(manifest);
        return manifest;

        static IEnumerable<PackageDependency> createDependencies(string[] dependencies)
        {
            foreach (var dep in dependencies)
            {
                yield return (PackageDependency)dep;
            }
        }
    }

    private static async Task<PackageContext> createPackageContextFromFilesAsync(params string[] paths)
    {
        foreach (var path in paths)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"File was not found: '{path}'.");
        }

        var scopePath = getScopePath();
        return await createContext(scopePath, client: null, paths);
    }

    private static string getScopePath()
    {
        var scopePath = Path.Combine(Path.GetTempPath(), "package-" + Path.GetRandomFileName());
        if (!Directory.Exists(scopePath))
        {
            Directory.CreateDirectory(scopePath);
        }
        return scopePath;
    }

    private static async Task<PackageContext> createContext(string scopePath, PackageClient? client, string[]? filePaths = null, bool localCache = false)
    {
        string? cacheFolder = localCache ? scopePath : null;
        var cache = new DiskPackageCache(cacheFolder);
        var project = new FolderProject(scopePath);
        var scope = new PackageContext(cache, project, client);

        //install packages from a package server
        if (client is not null)
        {
            var closure = await scope.Restore();

            if (closure.Missing.Any())
            {
                var missingDeps = string.Join(", ", closure.Missing);
                throw new FileNotFoundException($"Could not resolve all dependencies. Missing: {missingDeps}.");
            }
        }
        //install packages from local machine
        if (filePaths is not null && filePaths.Any())
        {
            foreach (var path in filePaths)
            {
                await installPackageFromPath(scope, path);
            }
        }


        return scope;
    }

    private static async Task installPackageFromPath(PackageContext scope, string path)
    {
        var packageManifest = Packaging.ExtractManifestFromPackageFile(path);
        if (packageManifest is not null)
        {
            var reference = packageManifest.GetPackageReference();
            await scope.Cache.Install(reference, path);

            var dependency = new PackageDependency(reference.Name ?? "", reference.Version);
            if (reference.Found)
            {
                var manifest = await scope.Project.ReadManifest();
                var fhirVersion = packageManifest.GetFhirVersion();
                if (fhirVersion is null)
                {
                    throw new("Manifest doesn't contain a valid FHIR version");
                }
                manifest ??= ManifestFile.Create("temp", fhirVersion);
                if (manifest.Name == reference.Name)
                {
                    throw new("Skipped updating package manifest because it would cause the package to reference itself.");
                }
                else
                {
                    manifest.AddDependency(dependency);
                    await scope.Project.WriteManifest(manifest);
                    await scope.Restore();
                }
            }
        }

    }

    private Resource? toResource(string content)
    {
        try
        {
            var sourceNode = content.StartsWith("{")
                ? FhirJsonNode.Parse(content)
                : FhirXmlNode.Parse(content);

            return sourceNode.ToPoco(_provider) as Resource;
        }
        catch
        {
            return null;
        }
    }

    ///<inheritdoc/>
    public async Task<Resource?> ResolveByCanonicalUriAsync(string uri)
    {
        var content = await ResolveByCanonicalUriAsyncAsString(uri).ConfigureAwait(false);
        return content is null ? null : toResource(content);
    }

    //internal for test purposes
    internal async Task<string?> ResolveByCanonicalUriAsyncAsString(string uri)
    {
        var (url, version) = splitCanonical(uri);
        return await _context.Value.GetFileContentByCanonical(url, version, resolveBestCandidate: true).ConfigureAwait(false);
    }


    private static (string url, string version) splitCanonical(string canonical)
    {
        if (canonical.EndsWith("|"))
            canonical = canonical.Substring(0, canonical.Length - 1);

        var position = canonical.LastIndexOf('|');

        return position == -1 ?
            (canonical, "")
            : (canonical.Substring(0, position), canonical.Substring(position + 1));
    }


    ///<inheritdoc/>
    public async Task<Resource?> ResolveByUriAsync(string uri)
    {
        var content = await ResolveByUriAsyncAsString(uri).ConfigureAwait(false);
        return content is null ? null : toResource(content);
    }

    //internal for test purposes
    internal async Task<string?> ResolveByUriAsyncAsString(string uri)
    {
        uri.SplitLeft('/').Deconstruct(out var resource, out var id);

        if (resource == null || id is null)
            return null;

        return await _context.Value.GetFileContentById(resource, id).ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public IEnumerable<string> ListArtifactNames()
    {
        return _context.Value.GetFileNames();
    }

    ///<inheritdoc/>
    public Stream? LoadArtifactByName(string artifactName)
    {
        var content = TaskHelper.Await(() => _context.Value.GetFileContentByFileName(artifactName));
        return content == null ? null : new MemoryStream(Encoding.UTF8.GetBytes(content));
    }

    ///<inheritdoc/>
    public Stream? LoadArtifactByPath(string artifactPath)
    {
        var content = TaskHelper.Await(() => _context.Value.GetFileContentByFilePath(artifactPath));
        return content == null ? null : new MemoryStream(Encoding.UTF8.GetBytes(content));
    }

    ///<inheritdoc/>
    public IEnumerable<string> ListResourceUris(string? filter = null)
    {
        return _context.Value.ListResourceUris(filter);
    }


    /// <summary>
    /// Lists all canonical Uri's of a package, with optional filter on resource type
    /// </summary>
    /// <param name="filter">Resource type as string used to filter</param>
    /// <returns>Sequence of canonical uri strings.</returns>
    public IEnumerable<string> ListCanonicalUris(string? filter = null)
    {
        return _context.Value.ListCanonicalUris(filter);
    }

    ///<inheritdoc/>
    public async Task<Resource?> FindCodeSystemByValueSet(string valueSetUri)
    {
        var content = await FindCodeSystemByValueSetAsString(valueSetUri).ConfigureAwait(false);
        return content is null ? null : toResource(content);
    }

    //internal for test purposes
    internal async Task<string?> FindCodeSystemByValueSetAsString(string valueSetUri)
    {
        return await _context.Value.GetCodeSystemByValueSet(valueSetUri).ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public async Task<IEnumerable<Resource>> FindConceptMaps(string? sourceUri = null, string? targetUri = null)
    {
        var content = await FindConceptMapsAsStrings(sourceUri, targetUri).ConfigureAwait(false);
        return from item in content
            let resource = toResource(item)
            where resource is not null
            select resource;
    }

    //internal for test purposes
    internal async Task<IEnumerable<string>> FindConceptMapsAsStrings(string? sourceUri = null, string? targetUri = null)
    {
        var cms = await _context.Value.GetConceptMapsBySourceAndTarget(sourceUri, targetUri).ConfigureAwait(false);
        return cms is not null ? cms : Enumerable.Empty<string>();
    }

    ///<inheritdoc/>
    public async Task<Resource?> FindNamingSystemByUniqueId(string uniqueId)
    {
        var content = await FindNamingSystemByUniqueIdAsString(uniqueId).ConfigureAwait(false);
        return content is null ? null : toResource(content);
    }

    //internal for test purposes
    internal async Task<string?> FindNamingSystemByUniqueIdAsString(string uniqueId)
    {
        return await _context.Value.GetNamingSystemByUniqueId(uniqueId).ConfigureAwait(false);
    }
}