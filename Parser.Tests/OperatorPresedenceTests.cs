

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
        private static Constant True = Constant.True;
        private static Constant One = new Constant(1, Types.Int);

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

        public static readonly object[][] TestCases = new object[][]
        {
            new object[] { "true | true & true", new OrElse(True, new AndAlso(True, True)) },
            new object[] { "true & true | true", new OrElse(new AndAlso(True, True), True) },
            new object[] { "true = true & true = true", new AndAlso(new Equal(True, True), new Equal(True, True))},
            new object[] { "true = (true & true) = true", new Equal(True, new Equal(new AndAlso(True, True), True))},
            new object[] { "1 * 1 + 1", new Add(new Multiply(One, One, Types.Int), One, Types.Int)},
            new object[] { "1 + 1 * 1", new Add(One, new Multiply(One, One, Types.Int), Types.Int)},
            new object[] { "1 * 1 = 1 + 1", new Equal(new Multiply(One, One, Types.Int), new Add(One, One, Types.Int))},
            new object[] { "1 * 1 = 1 + 1 & 1 * 1 = 1 + 1", new AndAlso(new Equal(new Multiply(One, One, Types.Int), new Add(One, One, Types.Int)), new Equal(new Multiply(One, One, Types.Int), new Add(One, One, Types.Int)))},
            new object[] { "+1 + -1", new Add(new Plus(One), new Minus(One), Types.Int)},
            new object[] { "-(+1 + -1)", new Minus(new Add(new Plus(One), new Minus(One), Types.Int))},
            new object[] { "[1][1]", new IndexDereference(new global::Parser.Expressions.Array(new[] { One }, new ArrayTypeShim(new int[] { 2 }, Types.Int, false)), new[] { One })}
        };

        [TestCaseSource(nameof(TestCases))]
        public void ExpressionTests(string expression, Expression expectedResult)
        {
            // Arrange
            var parser = CreateParser<BinaryParser>(expression);

            // Act
            var result = parser.Parse();

            // Assert
            Assert.That(result, ExpressionIs.EqualTo(expectedResult));
        }
    }
}
