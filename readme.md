## Introduction ##
This is Firely's support library for anyone who wants to work with [FHIR packages][packages-wiki].

We use this library in our own software including:
- [Simplifier.net][simplifier]
- [Firely .NET SDK][sdk]
- [Forge][forge] 
- and [Firely Terminal][terminal] 


## What's in the box?
This library provides:
* An NPM/FHIR-package client for resolving and publishing FHIR packages.
* Functionalities to create FHIR packages from files on disk.
* Installation of FHIR packages on your machine
* Helper classes to create the correct manifest and index files for FHIR packages

## Authenticating against a private package feed
Package servers that require authentication, such as private [Simplifier.net][simplifier] package feeds, are supported by
passing a token provider. The provider is a function that returns a Bearer token (for Simplifier, a JWT access token) and is
invoked for every request, so long-running applications can return a refreshed token once the previous one has expired.

A private Simplifier feed is addressed as `https://packages.simplifier.net/feeds/{feedname}`, and packages within it live at
`/{package}/{version}`.

```csharp
// A package client for a private feed:
var client = PackageClient.Create("https://packages.simplifier.net/feeds/myfeed",
    tokenProvider: _ => Task.FromResult(myJwtToken));

// Or resolve artifacts directly from packages on a private feed:
var resolver = new FhirPackageSource(ModelInfo.ModelInspector,
    "https://packages.simplifier.net/feeds/myfeed",
    ["mypackage@1.0.0"],
    tokenProvider: _ => Task.FromResult(myJwtToken));
```

Obtaining and refreshing the token itself (for Simplifier: the `/token` and `/token/refresh` endpoints) is the caller's
responsibility; this library only attaches the token to its requests.

## Nuget
You can use the library by downloading the [nuget package][nuget]

## Support 
You are welcome to register your bugs and feature suggestions at [https://github.com/FirelyTeam/Firely.Fhir.Packages/issues](https://github.com/FirelyTeam/Firely.Fhir.Packages/issues). 
For questions and broader discussions, we use the .NET FHIR Tooling chat on [Zulip][zulip].

## Contributing ##
We are welcoming contributors!

If you want to participate in this project, we're using [Git Flow][nvie] for our branch management, so please submit your commits using pull requests on the `develop` branch! 

### GIT branching strategy 
- [NVIE](http://nvie.com/posts/a-successful-git-branching-model/)
- Or see: [Git workflow](https://www.atlassian.com/git/workflows#!workflow-gitflow)

[packages-wiki]: https://confluence.hl7.org/display/FHIR/NPM+Package+Specification
[simplifier]: http://simplifier.net
[sdk]: http://github.com/firelyteam/firely-net-sdk
[terminal]: https://fire.ly/products/firely-terminal/
[forge]: https://fire.ly/products/forge/
[zulip]: https://chat.fhir.org/#narrow/stream/179239-tooling
[nuget]: https://www.nuget.org/packages/Firely.Fhir.Packages
[nvie]: http://nvie.com/posts/a-successful-git-branching-model/