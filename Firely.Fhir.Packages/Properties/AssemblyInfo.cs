/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */

using System;
using System.Runtime.CompilerServices;

[assembly: CLSCompliant(true)]

#if DEBUG
[assembly: InternalsVisibleTo("Firely.Fhir.Packages.Tests")]
#endif

#if RELEASE
// https://docs.microsoft.com/en-us/dotnet/standard/assembly/create-signed-friend
[assembly: InternalsVisibleTo("Firely.Fhir.Packages.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100f159829565775710a5baa5bb45cd80aabd633478bc2b5a180939336ca884181152db2b507ffec768d01372005f851df65546707a0f3ee530af138f7cfa9dd5d73997820c385fd166c0eae480bdf14920d5780fb1163da88fbb09f97dd4af4e3b55003051f01e47e0691e21e61d048fc95b2e10901b33f05699d8edd3207845d0")]
#endif