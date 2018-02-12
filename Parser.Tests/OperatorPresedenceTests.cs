

namespace Parser.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using NUnit.Framework;

    using global::Parser.Expressions;
    using global::Parser.Components;
    using static Components.ComponentHelper;

    [TestFixture]
    public class OperatorPresedenceTests
    {
        [Test]
        public void Logical_Ok()
        {
            // Arrange
            var parser = CreateParser<BinaryParser>("true = false && true = true");
            var expectedResult = new AndAlso(new Equal(Constant.True, Constant.False), new Equal(Constant.True, Constant.True));

            // Act
            var result = parser.Parse();

            // Assert
            Assert.That(result, ExpressionIs.EqualTo(expectedResult));
        }
    }
}
