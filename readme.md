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