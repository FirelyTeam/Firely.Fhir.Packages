/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Firely.Fhir.Packages;

internal class SemVerCanonicalVersionResolver : ICanonicalVersionResolver
{
    public VersionAlgorithm Algorithm => VersionAlgorithm.SemVer;

    public bool Fits(IReadOnlyList<string> versions) => versions.All(v => SemanticVersioning.Version.TryParse(v, out _));

    public int Compare(string a, string b) => parse(a, nameof(a)).CompareTo(parse(b, nameof(b)));

    private static SemanticVersioning.Version parse(string version, string paramName) =>
        SemanticVersioning.Version.TryParse(version, out var parsed)
            ? parsed
            : throw new ArgumentException($"'{version}' is not a valid SemVer version.", paramName);
}
