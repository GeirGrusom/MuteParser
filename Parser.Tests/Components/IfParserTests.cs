using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Tests.Components
{
    using NUnit.Framework;

    using global::Parser.Expressions;
    using global::Parser.Components;
    using global::Parser.SyntaxTrivia;

    using static ComponentHelper;

    [TestFixture]
    public class IfParserTests
    {
        [Test]
        public void IfTrue_Ok()
        {
            // Arrange
            var ifParser = CreateParser<IfParser>("if true { } else { }");

            // Act
            var result = (If)ifParser.Parse();

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void IfTrue_VariableAssignedInBoth_ProducesVariableAssignedTrivia()
        {
            // Arrange
            var variable = new Variable("a", true, Types.Int);
            var ifParser = CreateParser<IfParser>("if true { a <- 10 } else { a <- 20 }", variable);

            // Act
            var result = (If)ifParser.Parse();

            // Assert
            Assert.That(result.Trivia, Has.One.InstanceOf<VariableAssignedTrivia>());
        }

        [Test]
        public void IfTrue_VariableAssignedInTrue_DoesNotProduceVariableAssignedTrivia()
        {
            // Arrange
            var variable = new Variable("a", true, Types.Int);
            var ifParser = CreateParser<IfParser>("if true { a <- 10 } else { }", variable);

            // Act
            var result = (If)ifParser.Parse();

            // Assert
            Assert.That(result.Trivia, Has.No.InstanceOf<VariableAssignedTrivia>());
        }

        [Test]
        public void IfTrue_VariableAssignedInFalse_DoesNotProduceVariableAssignedTrivia()
        {
            // Arrange
            var variable = new Variable("a", true, Types.Int);
            var ifParser = CreateParser<IfParser>("if true { } else { a <- 20 }", variable);

            // Act
            var result = (If)ifParser.Parse();

            // Assert
            Assert.That(result.Trivia, Has.No.InstanceOf<VariableAssignedTrivia>());
        }

        [Test]
        public void IfTrue_NullCheck_AndAlso_ProducesNotNullTrivia()
        {
            // Arrange
            var variableA = new Variable("a", false, Types.GetTypeShim(typeof(string), true));
            var variableB = new Variable("b", false, Types.GetTypeShim(typeof(string), true));
            var ifParser = CreateParser<IfParser>("if a != null & b != null { } else { }", variableA, variableB);

            // Act
            var result = (If)ifParser.Parse();

            // Assert
            Assert.That(result.True.Trivia, Has.One.EqualTo(new VariableIsNotNullTrivia(variableA)));
            Assert.That(result.True.Trivia, Has.One.EqualTo(new VariableIsNotNullTrivia(variableB)));
        }

        [Test]
        public void VariableNotEqual()
        {
            // Arrange
            var variableA = new Variable("a", false, Types.GetTypeShim(typeof(string), true));
            var ifParser = CreateParser<EqualParser>("a != null", variableA);

            // Act
            var result = (NotEqual)ifParser.Parse();

            // Assert
            Assert.That(result, ExpressionIs.EqualTo(new NotEqual(variableA, Constant.Null)));
        }

        [Test]
        public void IfTrue_NullCheck_Not_ProducesNotNullTrivia()
        {
            // Arrange
            var variableA = new Variable("a", false, Types.GetTypeShim(typeof(string), true));
            //var variableB = new Variable("b", false, Types.GetTypeShim(typeof(string), true));
            var ifParser = CreateParser<IfParser>("if !(a = null) { } else { }", variableA);

            // Act
            var result = (If)ifParser.Parse();

            // Assert
            Assert.That(result.True.Trivia, Has.One.EqualTo(new VariableIsNotNullTrivia(variableA)));
        }
    }
}
