using FluentAssertions;
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Specification;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace Firely.Fhir.Packages.Tests;

/// <summary>Tests for the location of the package cache: explicit folder, environment variable, user profile and common application data fallback. None of these need network access.</summary>
[TestClass]
public class PackageCacheLocationTests
{
    private const string PACKAGE_FILE = "TestData/testPackage.tgz";
    private const string PACKAGE_FOLDER = "firely.sdk.testpackage#3.0.4";
    private const string PROFILE = "profile-home";
    private const string COMMON = "common-appdata";

    private string? _originalEnvVar;
    private string? _originalPackageRoot;
    private readonly List<string> _tempFolders = [];

    [TestInitialize]
    public void Init()
    {
        _originalEnvVar = Environment.GetEnvironmentVariable(Platform.PackageCacheEnvironmentVariable);
        _originalPackageRoot = Platform.PackageRoot;
        Environment.SetEnvironmentVariable(Platform.PackageCacheEnvironmentVariable, null);
        Platform.PackageRoot = null;
    }

    [TestCleanup]
    public void Cleanup()
    {
        Environment.SetEnvironmentVariable(Platform.PackageCacheEnvironmentVariable, _originalEnvVar);
        Platform.PackageRoot = _originalPackageRoot;
        foreach (var folder in _tempFolders.Where(Directory.Exists))
            Directory.Delete(folder, recursive: true);
    }

    private string newTempFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "fhir-cache-test-" + Path.GetRandomFileName());
        _tempFolders.Add(folder);
        return folder;
    }

    // Fakes the environment so the resolution can be tested without touching the real one.
    private static string resolve(string? packageRoot = null, string? envVar = null, bool hasProfile = true, bool hasCommonData = true)
    {
        string? getEnv(string name) => name switch
        {
            Platform.PackageCacheEnvironmentVariable => envVar,
            "UserProfile" or "HOME" => hasProfile ? PROFILE : null,
            _ => null
        };

        string getFolder(Environment.SpecialFolder folder) => folder switch
        {
            Environment.SpecialFolder.CommonApplicationData => hasCommonData ? COMMON : "",
            Environment.SpecialFolder.UserProfile => hasProfile ? PROFILE : "",
            _ => ""
        };

        return Platform.ResolveFhirPackageRoot(packageRoot, getEnv, getFolder);
    }

    [TestMethod]
    public void UserProfileIsUsedByDefault()
        => resolve().Should().Be(Path.Combine(PROFILE, ".fhir", "packages"));

    [TestMethod]
    public void EnvironmentVariableOverridesUserProfile()
        => resolve(envVar: "env-cache").Should().Be("env-cache");

    [TestMethod]
    public void StaticPackageRootOverridesEnvironmentVariable()
        => resolve(packageRoot: "static-cache", envVar: "env-cache").Should().Be("static-cache");

    [TestMethod]
    public void BlankOverridesAreIgnored()
        => resolve(packageRoot: " ", envVar: "").Should().Be(Path.Combine(PROFILE, ".fhir", "packages"));

    [TestMethod]
    public void CommonApplicationDataIsUsedWhenThereIsNoUserProfile()
        => resolve(hasProfile: false).Should().Be(Path.Combine(COMMON, ".fhir", "packages"));

    [TestMethod]
    public void EnvironmentVariableIsUsedEvenWithoutUserProfile()
        => resolve(envVar: "env-cache", hasProfile: false).Should().Be("env-cache");

    [TestMethod]
    public void ThrowsOnlyWhenNothingIsAvailable()
    {
        var act = () => resolve(hasProfile: false, hasCommonData: false);
        act.Should().Throw<Exception>().WithMessage($"*{Platform.PackageCacheEnvironmentVariable}*");
    }

    [TestMethod]
    public void GetFhirPackageRootReadsEnvironmentVariableAndStaticOverride()
    {
        Environment.SetEnvironmentVariable(Platform.PackageCacheEnvironmentVariable, "env-cache");
        Platform.GetFhirPackageRoot().Should().Be("env-cache");

        Platform.PackageRoot = "static-cache";
        Platform.GetFhirPackageRoot().Should().Be("static-cache");
    }

    [TestMethod]
    public void DiskPackageCacheUsesExplicitRootOverEnvironmentVariable()
    {
        Environment.SetEnvironmentVariable(Platform.PackageCacheEnvironmentVariable, "env-cache");

        new DiskPackageCache("explicit").Root.Should().Be("explicit");
        new DiskPackageCache().Root.Should().Be("env-cache");
    }

    [TestMethod]
    public async Task FhirPackageSourceInstallsIntoExplicitCacheFolder()
    {
        var explicitFolder = newTempFolder();
        var envFolder = newTempFolder();
        Environment.SetEnvironmentVariable(Platform.PackageCacheEnvironmentVariable, envFolder);

        var source = new FhirPackageSource(new ModelInspector(FhirRelease.STU3), [PACKAGE_FILE], explicitFolder);
        (await source.ResolveByCanonicalUriAsyncAsString("http://hl7.org/fhir/StructureDefinition/Patient")).Should().NotBeNull();

        Directory.Exists(Path.Combine(explicitFolder, PACKAGE_FOLDER)).Should().BeTrue("the explicit cache folder wins");
        Directory.Exists(envFolder).Should().BeFalse("the environment variable must not be used when a folder is passed");
    }

    [TestMethod]
    public async Task FhirPackageSourceInstallsIntoEnvironmentVariableFolder()
    {
        var envFolder = newTempFolder();
        Environment.SetEnvironmentVariable(Platform.PackageCacheEnvironmentVariable, envFolder);

        var source = new FhirPackageSource(new ModelInspector(FhirRelease.STU3), PACKAGE_FILE);
        (await source.ResolveByCanonicalUriAsyncAsString("http://hl7.org/fhir/StructureDefinition/Patient")).Should().NotBeNull();

        Directory.Exists(Path.Combine(envFolder, PACKAGE_FOLDER)).Should().BeTrue();
    }

    [TestMethod]
    public async Task FhirPackageSourceInstallsIntoStaticPackageRootFolder()
    {
        var staticFolder = newTempFolder();
        var envFolder = newTempFolder();
        Environment.SetEnvironmentVariable(Platform.PackageCacheEnvironmentVariable, envFolder);
        Platform.PackageRoot = staticFolder;

        var source = new FhirPackageSource(new ModelInspector(FhirRelease.STU3), PACKAGE_FILE);
        (await source.ResolveByCanonicalUriAsyncAsString("http://hl7.org/fhir/StructureDefinition/Patient")).Should().NotBeNull();

        Directory.Exists(Path.Combine(staticFolder, PACKAGE_FOLDER)).Should().BeTrue();
        Directory.Exists(envFolder).Should().BeFalse();
    }
}
