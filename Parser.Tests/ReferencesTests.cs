using System;
using NUnit.Framework;

namespace Parser.Tests
{
    [TestFixture]
    public class ReferencesTests
    {
        [Test]
        public void TryFindType_ThisType_ReturnsType()
        {
            // Arrange
            var references = new References(new[] { GetType().Assembly });

            // Act
            var type = references.TryFindType(new[] { "Parser", "Tests" }, nameof(ReferencesTests));

            // Assert
            Assert.That(type, Is.Not.Null);
        }

        [Test]
        public void TryFindType_DoesNotExist_ReturnsNull()
        {
            // Arrange
            var references = new References(new[] { GetType().Assembly });

            // Act
            var type = references.TryFindType(new[] { "NotParser", "Tests" }, nameof(ReferencesTests));

            // Assert
            Assert.That(type, Is.Null);
        }
    }
}
