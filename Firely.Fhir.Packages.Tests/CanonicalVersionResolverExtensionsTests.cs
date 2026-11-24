using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Firely.Fhir.Packages.Tests
{
    [TestClass]
    public class CanonicalVersionResolverExtensionsTests
    {
        private sealed record Item(string? Version, bool Preferred, string Name);

        private static ICanonicalVersionResolver R(VersionAlgorithm a) => CanonicalReferenceHandling.VersionResolvers[a];

        private static Item[] Items(params (string? v, bool p)[] xs)
            => xs.Select((x, i) => new Item(x.v, x.p, "i" + i)).ToArray();

        [TestMethod]
        public void VersionResolvers_has_one_matching_entry_per_algorithm()
        {
            var all = Enum.GetValues<VersionAlgorithm>();
            CanonicalReferenceHandling.VersionResolvers.Keys.Should().BeEquivalentTo(all);
            foreach (var a in all)
                CanonicalReferenceHandling.VersionResolvers[a].Algorithm.Should().Be(a);
        }

        [TestMethod]
        public void VersionResolvers_cannot_be_modified_through_a_mutable_cast()
        {
            var before = CanonicalReferenceHandling.VersionResolvers.ToArray();
            if (CanonicalReferenceHandling.VersionResolvers is IDictionary<VersionAlgorithm, ICanonicalVersionResolver> d)
            {
                var act = () => { d.Remove(VersionAlgorithm.Date); d[VersionAlgorithm.Natural] = d[VersionAlgorithm.Alpha]; d.Clear(); };
                act.Should().Throw<NotSupportedException>();
            }
            CanonicalReferenceHandling.VersionResolvers.ToArray().Should().Equal(before);
        }

        [DataRow("semver", VersionAlgorithm.SemVer)]
        [DataRow("integer", VersionAlgorithm.Integer)]
        [DataRow("alpha", VersionAlgorithm.Alpha)]
        [DataRow("date", VersionAlgorithm.Date)]
        [DataRow("natural", VersionAlgorithm.Natural)]
        [DataTestMethod]
        public void TryParseVersionAlgorithm_accepts_the_five_lowercase_codes(string code, VersionAlgorithm expected)
        {
            CanonicalReferenceHandling.TryParseVersionAlgorithm(code, out var a).Should().BeTrue();
            a.Should().Be(expected);
        }

        [DataRow("SemVer")]
        [DataRow("")]
        [DataRow(null)]
        [DataRow("x")]
        [DataRow(" semver")]
        [DataTestMethod]
        public void TryParseVersionAlgorithm_rejects_other_input(string? code)
            => CanonicalReferenceHandling.TryParseVersionAlgorithm(code, out _).Should().BeFalse();

        [DataRow(VersionAlgorithm.SemVer, new[] { "1.9.0", "1.10.0" }, "1.10.0")]
        [DataRow(VersionAlgorithm.Natural, new[] { "1.9", "1.10" }, "1.10")]
        [DataRow(VersionAlgorithm.Alpha, new[] { "a", "b", "10" }, "b")]
        [DataRow(VersionAlgorithm.Integer, new[] { "2", "10" }, "10")]
        [DataRow(VersionAlgorithm.Date, new[] { "20240101", "2024-02-01" }, "2024-02-01")]
        [DataRow(VersionAlgorithm.Natural, new[] { "1.01", "1.1" }, "1.01")]
        [DataRow(VersionAlgorithm.Natural, new[] { "1.1", "1.01" }, "1.1")]
        [DataTestMethod]
        public void HighestVersion_returns_the_highest_and_the_first_of_equals(VersionAlgorithm a, string[] versions, string expected)
            => R(a).HighestVersion(versions).Should().Be(expected);

        [TestMethod]
        public void HighestVersion_ignores_missing_versions()
            => R(VersionAlgorithm.SemVer).HighestVersion(new string?[] { null, "1.0.0", "" }).Should().Be("1.0.0");

        [TestMethod]
        public void HighestVersion_is_null_for_empty_or_all_missing()
        {
            R(VersionAlgorithm.SemVer).HighestVersion(Array.Empty<string?>()).Should().BeNull();
            R(VersionAlgorithm.SemVer).HighestVersion(new string?[] { null, "" }).Should().BeNull();
        }

        [DataRow(VersionAlgorithm.SemVer, new[] { "1.0.0", "draft" })]
        [DataRow(VersionAlgorithm.Integer, new[] { "1.0" })]
        [DataRow(VersionAlgorithm.Date, new[] { "2024-99" })]
        [DataTestMethod]
        public void Highest_and_SelectBest_throw_ArgumentException_for_a_version_that_does_not_fit(VersionAlgorithm a, string[] versions)
        {
            var r = R(a);
            var h = () => r.HighestVersion(versions);
            h.Should().Throw<ArgumentException>();
            var s = () => r.SelectBestCandidate(versions, v => v);
            s.Should().Throw<ArgumentException>();
        }

        [DataRow(VersionAlgorithm.Natural)]
        [DataRow(VersionAlgorithm.Alpha)]
        [DataTestMethod]
        public void Natural_and_Alpha_accept_any_strings(VersionAlgorithm a)
        {
            var versions = new[] { "draft", "1.0", "2024-99", "١" };
            var r = R(a);
            var h = () => r.HighestVersion(versions);
            h.Should().NotThrow();
            var s = () => r.SelectBestCandidate(versions, v => v);
            s.Should().NotThrow();
        }

        [TestMethod]
        public void Null_arguments_throw_ArgumentNullException()
        {
            var r = R(VersionAlgorithm.SemVer);
            var items = new[] { "1.0.0" };
            ((Action)(() => ((ICanonicalVersionResolver)null!).HighestVersion(items))).Should().Throw<ArgumentNullException>();
            ((Action)(() => r.HighestVersion(null!))).Should().Throw<ArgumentNullException>();
            ((Action)(() => ((ICanonicalVersionResolver)null!).SelectBestCandidate(items, v => v))).Should().Throw<ArgumentNullException>();
            ((Action)(() => r.SelectBestCandidate<string>(null!, v => v))).Should().Throw<ArgumentNullException>();
            ((Action)(() => r.SelectBestCandidate(items, null!))).Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void SelectBestCandidate_throws_InvalidOperation_when_empty()
        {
            var act = () => R(VersionAlgorithm.SemVer).SelectBestCandidate(Array.Empty<Item>(), i => i.Version);
            act.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void SelectBestCandidate_all_missing_prefers_the_preferred_in_any_position()
        {
            var r = R(VersionAlgorithm.SemVer);
            foreach (var pos in new[] { 0, 1, 2 })
            {
                var items = Items((null, pos == 0), ("", pos == 1), (null, pos == 2));
                r.SelectBestCandidate(items, i => i.Version, i => i.Preferred).Should().BeSameAs(items[pos]);
            }
        }

        [TestMethod]
        public void SelectBestCandidate_all_missing_without_preferred_returns_the_first()
        {
            var r = R(VersionAlgorithm.SemVer);
            var items = Items((null, false), ("", false));
            r.SelectBestCandidate(items, i => i.Version).Should().BeSameAs(items[0]);
            r.SelectBestCandidate(items, i => i.Version, null).Should().BeSameAs(items[0]);
        }

        [TestMethod]
        public void SelectBestCandidate_missing_version_never_beats_a_present_one()
        {
            var items = Items((null, true), ("0.0.1", false));
            R(VersionAlgorithm.SemVer).SelectBestCandidate(items, i => i.Version, i => i.Preferred).Should().BeSameAs(items[1]);
        }

        [TestMethod]
        public void SelectBestCandidate_ties_go_to_the_first_preferred_else_the_first()
        {
            var r = R(VersionAlgorithm.Natural);
            var items = Items(("1.01", false), ("1.1", true), ("1.001", true), ("1.0", false));
            r.SelectBestCandidate(items, i => i.Version, i => i.Preferred).Should().BeSameAs(items[1]);
            r.SelectBestCandidate(items, i => i.Version).Should().BeSameAs(items[0]);
        }

        [DataRow("semver", new[] { "1.9.0", "1.10.0", "1.10.0-rc" })]
        [DataRow("integer", new[] { "2", "10", "010" })]
        [DataRow("alpha", new[] { "a", "b", "10" })]
        [DataRow("date", new[] { "20240101", "2024-02-01", "2024-03-01" })]
        [DataRow("natural", new[] { "1.9", "1.10", "1.010", "draft" })]
        [DataTestMethod]
        public void SelectBestCandidate_matches_FileIndex_for_each_code(string code, string[] versions)
        {
            CanonicalReferenceHandling.TryParseVersionAlgorithm(code, out var a).Should().BeTrue();
            var items = versions.Select((v, i) => new Item(v, i == 0, "i" + i)).ToArray();
            var viaIndex = FileIndex.SelectBestCandidate(items, i => i.Version, _ => code, i => i.Preferred);
            R(a).SelectBestCandidate(items, i => i.Version, i => i.Preferred).Should().BeSameAs(viaIndex);
        }

        [TestMethod]
        public void ResolverFor_null_throws_ArgumentNullException()
        {
            var act = () => CanonicalReferenceHandling.ResolverFor(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [DataRow(new[] { "1.0.0", null })]
        [DataRow(new string?[] { null })]
        [DataTestMethod]
        public void ResolverFor_null_element_throws_ArgumentException_but_not_ArgumentNullException(string?[] versions)
        {
            var act = () => CanonicalReferenceHandling.ResolverFor(versions!);
            act.Should().Throw<ArgumentException>().Which.Should().NotBeOfType<ArgumentNullException>();
        }

        [TestMethod]
        public void ResolverFor_accepts_an_empty_string_element()
        {
            var act = () => CanonicalReferenceHandling.ResolverFor(new[] { "1.0.0", "" });
            act.Should().NotThrow();
        }

        [DataRow(new[] { "1.9.0", "1.10.0" }, VersionAlgorithm.SemVer)]
        [DataRow(new[] { "2024-01-01", "2024-02-01" }, VersionAlgorithm.Date)]
        [DataRow(new[] { "1.9", "1.10" }, VersionAlgorithm.Natural)]
        [DataTestMethod]
        public void ResolverFor_infers_the_algorithm(string[] versions, VersionAlgorithm expected)
            => CanonicalReferenceHandling.ResolverFor(versions).Algorithm.Should().Be(expected);

        [TestMethod]
        public void SemVer_Compare_throws_ArgumentException_for_a_non_semver_argument()
        {
            var r = R(VersionAlgorithm.SemVer);
            var act = () => r.Compare("1.0.0", "draft");
            act.Should().Throw<ArgumentException>();
        }

        [DataRow(VersionAlgorithm.Integer)]
        [DataRow(VersionAlgorithm.Alpha)]
        [DataRow(VersionAlgorithm.Date)]
        [DataRow(VersionAlgorithm.Natural)]
        [DataTestMethod]
        public void Compare_does_not_throw_on_arbitrary_strings(VersionAlgorithm a)
        {
            var r = R(a);
            var act = () => { r.Compare("draft", "1.0"); r.Compare("1.0", "draft"); };
            act.Should().NotThrow();
        }

        [TestMethod]
        public void Example_semver_highest_version()
        {
            var semver = CanonicalReferenceHandling.VersionResolvers[VersionAlgorithm.SemVer];
            string? latest = semver.HighestVersion(new[] { "1.9.0", "1.10.0" });
            latest.Should().Be("1.10.0");
        }

        [TestMethod]
        public void Example_natural_select_best_candidate_with_preferred()
        {
            var files = new[] { new File("1.10", false), new File("1.10", true), new File("1.9", true) };
            var best = CanonicalReferenceHandling.VersionResolvers[VersionAlgorithm.Natural]
                .SelectBestCandidate(files, f => f.Version, f => f.HasSnapshot);
            best.Should().BeSameAs(files[1]);
        }

        [TestMethod]
        public void Example_parsed_algorithm_highest_version()
        {
            string? latest = null;
            if (CanonicalReferenceHandling.TryParseVersionAlgorithm("integer", out var algorithm))
                latest = CanonicalReferenceHandling.VersionResolvers[algorithm].HighestVersion(new[] { "2", "10" });
            latest.Should().Be("10");
        }

        [TestMethod]
        public void Example_inferred_resolver_highest_version()
        {
            var versions = new[] { "1.9.0", "1.10.0" };
            string? latest = CanonicalReferenceHandling.ResolverFor(versions).HighestVersion(versions);
            latest.Should().Be("1.10.0");
        }

        private sealed record File(string? Version, bool HasSnapshot);
    }
}
