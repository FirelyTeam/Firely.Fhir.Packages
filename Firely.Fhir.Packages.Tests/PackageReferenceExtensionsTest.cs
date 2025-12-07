using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Firely.Fhir.Packages.Tests
{
    [TestClass]
    public class PackageReferenceExtensionsTest
    {
        [TestMethod]
        public void ToDictionary_WithDuplicateKeys_ShouldNotThrowException()
        {
            // Arrange - Create a list with duplicate package names but different versions
            var references = new List<PackageReference>
            {
                new PackageReference("hl7.terminology.r4", "1.0.0"),
                new PackageReference("hl7.terminology.r4", "2.0.0"),
                new PackageReference("hl7.fhir.r4.core", "4.0.1")
            };

            // Act - This should not throw an exception
            Action act = () => references.ToDictionary();

            // Assert
            act.Should().NotThrow<ArgumentException>();
        }

        [TestMethod]
        public void ToDictionary_WithDuplicateKeys_ShouldKeepHighestVersion()
        {
            // Arrange - Create a list with duplicate package names but different versions
            var references = new List<PackageReference>
            {
                new PackageReference("hl7.terminology.r4", "1.0.0"),
                new PackageReference("hl7.terminology.r4", "2.0.0"),
                new PackageReference("hl7.fhir.r4.core", "4.0.1")
            };

            // Act
            var dict = references.ToDictionary();

            // Assert
            dict.Should().ContainKey("hl7.terminology.r4");
            dict["hl7.terminology.r4"].Should().Be("2.0.0", "the highest version should be kept");
            dict.Should().ContainKey("hl7.fhir.r4.core");
            dict["hl7.fhir.r4.core"].Should().Be("4.0.1");
        }

        [TestMethod]
        public void ToDictionary_WithDuplicateKeysReversed_ShouldKeepHighestVersion()
        {
            // Arrange - Create a list with duplicate package names in reverse order
            var references = new List<PackageReference>
            {
                new PackageReference("hl7.terminology.r4", "2.0.0"),
                new PackageReference("hl7.terminology.r4", "1.0.0"),
                new PackageReference("hl7.fhir.r4.core", "4.0.1")
            };

            // Act
            var dict = references.ToDictionary();

            // Assert
            dict.Should().ContainKey("hl7.terminology.r4");
            dict["hl7.terminology.r4"].Should().Be("2.0.0", "the highest version should be kept regardless of order");
            dict.Should().ContainKey("hl7.fhir.r4.core");
            dict["hl7.fhir.r4.core"].Should().Be("4.0.1");
        }

        [TestMethod]
        public void ToDictionary_WithNoDuplicates_ShouldWorkAsExpected()
        {
            // Arrange
            var references = new List<PackageReference>
            {
                new PackageReference("hl7.terminology.r4", "1.0.0"),
                new PackageReference("hl7.fhir.r4.core", "4.0.1")
            };

            // Act
            var dict = references.ToDictionary();

            // Assert
            dict.Should().HaveCount(2);
            dict["hl7.terminology.r4"].Should().Be("1.0.0");
            dict["hl7.fhir.r4.core"].Should().Be("4.0.1");
        }
    }
}
