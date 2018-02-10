using NUnit.Framework;
using Parser.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Tests.Expressions
{
    [TestFixture]
    public class ExpressionEqualityVisitorTests
    {
        [Test]
        public void Binary_Equal_ReturnsTrue()
        {
            // Arrange
            var visitor = new ExpressionEqualityVisitor();
            var left = new Add(Constant.False, Constant.False, Types.Bool);
            var right = new Add(Constant.False, Constant.False, Types.Bool);

            // Act
            var result = visitor.Visit(left, right);

            // Assert
            Assert.That(result, Is.EqualTo(Constant.True));
        }

        [Test]
        public void Binary_DifferentOperandValues_ReturnsFalse()
        {
            // Arrange
            var visitor = new ExpressionEqualityVisitor();
            var left = new Add(Constant.False, Constant.False, Types.Bool);
            var right = new Add(Constant.False, Constant.True, Types.Bool);

            // Act
            var result = visitor.Visit(left, right);

            // Assert
            Assert.That(result, Is.EqualTo(Constant.False));
        }

        [Test]
        public void Binary_DifferentOperatorTypes_ReturnsFalse()
        {
            // Arrange
            var visitor = new ExpressionEqualityVisitor();
            var left = new Add(Constant.False, Constant.False, Types.Bool);
            var right = new Multiply(Constant.False, Constant.True, Types.Bool);

            // Act
            var result = visitor.Visit(left, right);

            // Assert
            Assert.That(result, Is.EqualTo(Constant.False));
        }

        [Test]
        public void Binary_DifferentOperandTypes_ReturnsFalse()
        {
            // Arrange
            var visitor = new ExpressionEqualityVisitor();
            var left = new Add(Constant.False, Constant.False, Types.Bool);
            var right = new Add(Constant.Null, Constant.True, Types.Bool);

            // Act
            var result = visitor.Visit(left, right);

            // Assert
            Assert.That(result, Is.EqualTo(Constant.False));
        }
    }
}
