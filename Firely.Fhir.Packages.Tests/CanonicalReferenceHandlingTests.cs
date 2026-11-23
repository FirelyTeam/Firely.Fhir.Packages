using FluentAssertions;
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

        [DataRow(new[] { "1.9", "1.10" }, true)]
        [DataRow(new[] { "4.0.1.2", "4.0.1" }, true)]
        [DataRow(new[] { "1.0.0-ballot" }, false)]
        [DataRow(new[] { "draft" }, false)]
        [DataRow(new[] { "1", "2" }, false)]
        [DataRow(new[] { "1.2.3.4.5" }, false)]
        [DataTestMethod]
        public void Numeric_matches_only_dotted_numeric_with_two_to_four_parts(string[] versions, bool expected)
            => new NumericCanonicalVersionResolver().MatchThisAlgorithm(versions).Should().Be(expected);

        [TestMethod]
        public void Numeric_compares_parts_as_numbers_and_longer_is_greater()
        {
            var r = new NumericCanonicalVersionResolver();
            r.Compare("1.10", "1.9").Should().BePositive();
            r.Compare("4.0.1.2", "4.0.1").Should().BePositive();
        }

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
            new NumericCanonicalVersionResolver().Algorithm.Should().Be(VersionAlgorithm.Numeric);
            new DateCanonicalVersionResolver().Algorithm.Should().Be(VersionAlgorithm.Date);
            new AlphabeticalCanonicalVersionResolver().Algorithm.Should().Be(VersionAlgorithm.Alpha);
        }

        [DataRow(new[] { "1.9.0", "1.10.0" }, "SemVer")]
        [DataRow(new[] { "1.0.0-ballot", "1.0.0" }, "SemVer")]
        [DataRow(new[] { "1.9", "1.10" }, "Numeric")]
        [DataRow(new[] { "1.9", "1.10.0" }, "Numeric")]
        [DataRow(new[] { "2024-01-01", "2024-02-01" }, "Date")]
        [DataRow(new[] { "1.9", "1.10", "draft" }, "Alpha")]
        [DataRow(new[] { "2.0.0", "2024-01-01" }, "Alpha")]
        [DataTestMethod]
        public void ResolverFor_picks_first_rule_every_version_satisfies(string[] versions, string expected)
            => CanonicalReferenceHandling.ResolverFor(versions).Algorithm.ToString().Should().Be(expected);

        [DataRow(null, "1.9", "1.10", "1.10")]
        [DataRow("", "1.9", "1.10", "1.10")]
        [DataRow("1.9", "1.10", null, "1.10")]
        [DataRow("1.9", "1.10", "", "1.10")]
        [DataRow(null, "1.9.0", "1.10.0", "1.10.0")]
        [DataRow("", "1.9.0", "1.10.0", "1.10.0")]
        [DataRow("1.9.0", "1.10.0", null, "1.10.0")]
        [DataRow("1.9.0", "1.10.0", "", "1.10.0")]
        [DataTestMethod]
        public void SelectBestCandidate_ignores_missing_versions_when_choosing_the_rule(string? v1, string? v2, string? v3, string expected)
        {
            var candidates = new[] { v1, v2, v3 };
            FileIndex.SelectBestCandidate(candidates, v => v).Should().Be(expected);
        }
    }
}
