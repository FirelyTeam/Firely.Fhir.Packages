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
Package servers that require authentication, such as private [Simplifier.net][simplifier] package feeds, can be accessed by
constructing a `PackageClient` with your own `HttpClient` and passing it to `FhirPackageSource`. Because you control the
`HttpClient`, any authentication scheme is possible: a Bearer token (for Simplifier, a JWT access token), basic
authentication, an API key header, or a custom `DelegatingHandler`.

A private Simplifier feed is addressed as `https://packages.simplifier.net/feeds/{feedname}`, and packages within it live at
`/{package}/{version}`.

```csharp
var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", myJwtToken);

var client = new PackageClient(new FhirPackageUrlProvider("https://packages.simplifier.net/feeds/myfeed"), httpClient);

var resolver = new FhirPackageSource(ModelInfo.ModelInspector, client, ["mypackage@1.0.0"]);
```

A few things to be aware of:

* **Lifetime**: `FhirPackageSource` downloads its packages lazily, on first use. Do not dispose the `PackageClient` (or the
  `HttpClient` it wraps) before the source has resolved its packages.
* **Token expiry**: a token set as a default request header is frozen at the moment you set it. That is fine for short-lived
  processes, but in a long-running application a JWT will expire. In that case, attach your own `DelegatingHandler` to the
  `HttpClient` that supplies (and refreshes) the token per request.
* **Caching**: downloaded packages are stored in the machine-wide FHIR package cache (`~/.fhir/packages`), keyed by package
  name and version only — the package server is not part of the key. This means a package that is already present in the
  cache (for example, downloaded earlier from the public registry) is used as-is without contacting your private feed, and
  conversely, packages downloaded from a private feed become available from the cache to other tools and users on the same
  machine, without authentication. Keep this in mind when working with private packages on shared machines.
* Obtaining and refreshing the token itself (for Simplifier: the `/token` and `/token/refresh` endpoints) is the caller's
  responsibility; this library only sends the credentials you configured with its requests.

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