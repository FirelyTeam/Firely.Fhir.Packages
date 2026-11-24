using FluentAssertions;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Firely.Fhir.Packages.Tests
{
    [TestClass]
    public class CanonicalReferenceHandlingTests
    {
        [DataRow(new[] { "1.9.0", "1.10.0" }, true)]
        [DataRow(new[] { "1.0.0-ballot", "1.0.0" }, true)]
        [DataRow(new[] { "1.9", "1.10" }, false)]
        [DataRow(new[] { "4.0.1.2", "4.0.1" }, false)]
        [DataRow(new[] { "1.0.0", "draft" }, false)]
        [DataTestMethod]
        public void SemVer_matches_only_when_every_version_is_strict_semver(string[] versions, bool expected)
            => new SemVerCanonicalVersionResolver().MatchThisAlgorithm(versions).Should().Be(expected);

        [TestMethod]
        public void SemVer_compares_numerically_and_ranks_release_above_prerelease()
        {
            var r = new SemVerCanonicalVersionResolver();
            r.Compare("1.10.0", "1.9.0").Should().BePositive();
            r.Compare("1.0.0", "1.0.0-ballot").Should().BePositive();
        }

        [DataRow(new[] { "1", "2", "10" }, true)]
        [DataRow(new[] { "0010", "9" }, true)]
        [DataRow(new[] { "-1" }, false)]
        [DataRow(new[] { "1.0" }, false)]
        [DataRow(new[] { "\u0661" }, false)]
        [DataRow(new[] { "" }, false)]
        [DataRow(new[] { "1", "" }, false)]
        [DataTestMethod]
        public void Integer_matches_only_non_empty_ascii_digits(string[] versions, bool expected)
            => new IntegerCanonicalVersionResolver().MatchThisAlgorithm(versions).Should().Be(expected);

        [DataRow("9", "10", -1)]
        [DataRow("0010", "9", 1)]
        [DataRow("007", "7", 0)]
        [DataRow("1000000000000000000000000", "999999999999999999999999", 1)]
        [DataTestMethod]
        public void Integer_compares_numerically_at_any_length(string a, string b, int sign)
            => Math.Sign(new IntegerCanonicalVersionResolver().Compare(a, b)).Should().Be(sign);

        [TestMethod]
        public void Natural_matches_anything()
            => new NaturalCanonicalVersionResolver().MatchThisAlgorithm(new[] { "1.9", "draft", "2024-01-01", "" }).Should().BeTrue();

        [DataRow("1.10", "1.9", 1)]
        [DataRow("4.0.1.2", "4.0.1", 1)]
        [DataRow("v10", "v2", 1)]
        [DataRow("1.01", "1.1", 0)]
        [DataRow("Draft", "draft", 0)]
        [DataRow("draft", "1.0", 1)]
        [DataRow("1.0", "1.0.1", -1)]
        [DataRow("1.0", "1.0a", -1)]
        [DataRow("1.0.0", "1.0.0-ballot", -1)]
        [DataRow("1000000000000000000000000", "999999999999999999999999", 1)]
        [DataRow("b", "a", 1)]
        [DataTestMethod]
        public void Natural_compares_digit_runs_as_numbers_and_text_runs_ignoring_case(string a, string b, int sign)
            => Math.Sign(new NaturalCanonicalVersionResolver().Compare(a, b)).Should().Be(sign);

        [DataRow(new[] { "2024", "2025" }, true)]
        [DataRow(new[] { "2024-01", "2024-02" }, true)]
        [DataRow(new[] { "20240101", "2024-02-01" }, true)]
        [DataRow(new[] { "2024-99" }, false)]
        [DataRow(new[] { "2024-01-32" }, false)]
        [DataRow(new[] { "2024-0101" }, false)]
        [DataRow(new[] { "2024-02\n" }, false)]
        [DataRow(new[] { "٢٠٢٤" }, false)]
        [DataTestMethod]
        public void Date_matches_only_valid_dates_in_supported_formats(string[] versions, bool expected)
            => new DateCanonicalVersionResolver().MatchThisAlgorithm(versions).Should().Be(expected);

        [TestMethod]
        public void Date_orders_chronologically_and_ignores_separator_style()
        {
            var r = new DateCanonicalVersionResolver();
            r.Compare("2024-02-01", "2024-01-01").Should().BePositive();
            r.Compare("20240101", "2024-01-01").Should().Be(0);
        }

        [TestMethod]
        public void Alpha_matches_anything_and_compares_case_insensitively()
        {
            var r = new AlphabeticalCanonicalVersionResolver();
            r.MatchThisAlgorithm(new[] { "1.9", "draft", "2024-01-01" }).Should().BeTrue();
            r.Compare("draft", "DRAFT").Should().Be(0);
            r.Compare("b", "a").Should().BePositive();
        }

        [TestMethod]
        public void Resolvers_report_their_own_algorithm()
        {
            new SemVerCanonicalVersionResolver().Algorithm.Should().Be(VersionAlgorithm.SemVer);
            new IntegerCanonicalVersionResolver().Algorithm.Should().Be(VersionAlgorithm.Integer);
            new DateCanonicalVersionResolver().Algorithm.Should().Be(VersionAlgorithm.Date);
            new AlphabeticalCanonicalVersionResolver().Algorithm.Should().Be(VersionAlgorithm.Alpha);
            new NaturalCanonicalVersionResolver().Algorithm.Should().Be(VersionAlgorithm.Natural);
        }

        [DataRow(new[] { "1.9.0", "1.10.0" }, "SemVer")]
        [DataRow(new[] { "1.0.0-ballot", "1.0.0" }, "SemVer")]
        [DataRow(new[] { "2024-01-01", "2024-02-01" }, "Date")]
        [DataRow(new[] { "1.9", "1.10" }, "Natural")]
        [DataRow(new[] { "1.9", "1.10.0" }, "Natural")]
        [DataRow(new[] { "1.9", "1.10", "draft" }, "Natural")]
        [DataRow(new[] { "2.0.0", "2024-01-01" }, "Natural")]
        [DataRow(new[] { "1", "2", "10" }, "Natural")] // Integer is never inferred
        [DataTestMethod]
        public void ResolverFor_infers_first_rule_every_version_satisfies(string[] versions, string expected)
            => CanonicalReferenceHandling.ResolverFor(versions).Algorithm.ToString().Should().Be(expected);

        [DataRow("SemVer")]
        [DataRow("Integer")]
        [DataRow("Alpha")]
        [DataRow("Date")]
        [DataRow("Natural")]
        [DataTestMethod]
        public void VersionResolvers_lookup_returns_resolver_of_that_algorithm(string name)
        {
            var algorithm = Enum.Parse<VersionAlgorithm>(name);
            CanonicalReferenceHandling.VersionResolvers[algorithm].Algorithm.Should().Be(algorithm);
        }

        [DataRow(new[] { "1.0.0", "2.0.0" }, "semver", "SemVer")]
        [DataRow(new[] { "1", "2" }, "integer", "Integer")]
        [DataRow(new[] { "1.9", "1.10" }, "alpha", "Alpha")]
        [DataRow(new[] { "2024-01-01", "2024-02-01" }, "date", "Date")]
        [DataRow(new[] { "1.9", "1.10" }, "natural", "Natural")]
        [DataTestMethod]
        public void ResolverFor_uses_declared_algorithm_when_versions_fit(string[] versions, string code, string expected)
            => CanonicalReferenceHandling.ResolverFor(versions, new string?[] { code, code }).Algorithm.ToString().Should().Be(expected);

        [TestMethod]
        public void ResolverFor_falls_back_to_inference_when_declarations_conflict()
            => CanonicalReferenceHandling.ResolverFor(new[] { "1.9.0", "1.10.0" }, new string?[] { "alpha", "natural" }).Algorithm.Should().Be(VersionAlgorithm.SemVer);

        [TestMethod]
        public void ResolverFor_falls_back_to_inference_when_a_version_does_not_fit_the_declared_algorithm()
            => CanonicalReferenceHandling.ResolverFor(new[] { "1.9", "1.10" }, new string?[] { "semver" }).Algorithm.Should().Be(VersionAlgorithm.Natural);

        [DataRow("Semver")]
        [DataRow("numeric")]
        [DataRow("%version1 > %version2")]
        [DataTestMethod]
        public void ResolverFor_falls_back_to_inference_for_unknown_codes(string code)
            => CanonicalReferenceHandling.ResolverFor(new[] { "1.9.0", "1.10.0" }, new string?[] { code }).Algorithm.Should().Be(VersionAlgorithm.SemVer);

        [TestMethod]
        public void ResolverFor_falls_back_to_inference_when_all_declarations_are_empty()
            => CanonicalReferenceHandling.ResolverFor(new[] { "1.9.0", "1.10.0" }, new string?[] { null, "" }).Algorithm.Should().Be(VersionAlgorithm.SemVer);

        [TestMethod]
        public void ResolverFor_uses_the_single_declaration_when_others_are_undeclared()
            => CanonicalReferenceHandling.ResolverFor(new[] { "1.9.0", "1.10.0" }, new string?[] { "alpha", null }).Algorithm.Should().Be(VersionAlgorithm.Alpha);

        [DataRow(null, "1.9", "1.10", "1.10")]
        [DataRow("", "1.9", "1.10", "1.10")]
        [DataRow("1.9", "1.10", null, "1.10")]
        [DataRow("1.9", "1.10", "", "1.10")]
        [DataRow(null, "1.9.0", "1.10.0", "1.10.0")]
        [DataRow("", "1.9.0", "1.10.0", "1.10.0")]
        [DataRow("1.9.0", "1.10.0", null, "1.10.0")]
        [DataRow("1.9.0", "1.10.0", "", "1.10.0")]
        [DataRow(null, "1.0.0-ballot", "1.0.0", "1.0.0")]
        [DataRow("1.0.0-ballot", "1.0.0", null, "1.0.0")]
        [DataRow("", "1.0.0-ballot", "1.0.0", "1.0.0")]
        [DataRow("1.0.0-ballot", "1.0.0", "", "1.0.0")]
        [DataRow("", "2024-02", "202401", "2024-02")]
        [DataRow("2024-02", "202401", "", "2024-02")]
        [DataRow(null, "2024-02", "202401", "2024-02")]
        [DataRow("2024-02", "202401", null, "2024-02")]
        [DataTestMethod]
        public void SelectBestCandidate_ignores_missing_versions_when_choosing_the_rule(string? v1, string? v2, string? v3, string expected)
        {
            var candidates = new[] { v1, v2, v3 };
            FileIndex.SelectBestCandidate(candidates, v => v).Should().Be(expected);
        }
    }
}
