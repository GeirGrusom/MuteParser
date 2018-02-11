namespace Parser.Tests.Components
{
    using System.Linq;

    using SyntaxTrivia;

    using NUnit.Framework;

    using static ComponentHelper;

    using global::Parser.Components;
    using Assign = global::Parser.Expressions.Assign;
    using global::Parser.Expressions;

    [TestFixture]
    public class AssignParserTests
    {
        [Test]
        public void Assign_Parse_Ok()
        {            
            // Arrange
            var variable = new Variable("a", true, Types.Bool);
            var assignParser = CreateParser<AssignParser>("a <- true", variable);

            // Act
            var result = assignParser.Parse() as Assign;

            // Act
            Assert.That(result.Left, Is.SameAs(variable));
            assignParser.AssertStackIsEmpty();
        }

        [Test]
        public void Assign_Parse_Trivia_VariableAssigned()
        {
            // Arrange
            var variable = new Variable("a", true, Types.Bool);
            var assignParser = CreateParser<AssignParser>("a <- true", variable);

            // Act
            var result = assignParser.Parse() as Assign;

            // Act
            Assert.That(result.Trivia, Has.One.InstanceOf<VariableAssignedTrivia>());
            var assigned = result.Trivia.OfType<VariableAssignedTrivia>().Single();
            Assert.That(assigned.Variable, Is.SameAs(variable));
            assignParser.AssertStackIsEmpty();
        }

        [Test]
        public void Assign_MissingExpression_ReturnsNull()
        {
            // Arrange
            var variable = new Variable("a", true, Types.Bool);
            var assignParser = CreateParser<AssignParser>("a <- ", variable);

            // Act
            var result = assignParser.Parse() as Assign;

            // Act
            Assert.That(result, Is.Null);
            assignParser.AssertStackIsEmpty();
        }

        [Test]
        public void Assign_MissingTarget_ReturnsNull()
        {
            // Arrange
            var variable = new Variable("a", true, Types.Bool);
            var assignParser = CreateParser<AssignParser>("<- 100", variable);

            // Act
            var result = assignParser.Parse() as Assign;

            // Act
            Assert.That(result, Is.Null);
            assignParser.AssertStackIsEmpty();
        }

        [Test]
        public void Assign_ProducesVariableAssignedTrivia()
        {
            // Arrange
            var variable = new Variable("a", true, Types.GetTypeShim(typeof(bool), true));
            var assignParser = CreateParser<AssignParser>("a <- true", variable);

            // Act
            var result = assignParser.Parse() as Assign;

            // Act
            Assert.That(result.Trivia, Has.One.InstanceOf<VariableAssignedTrivia>());
            assignParser.AssertStackIsEmpty();
        }

        [Test]
        public void Assign_DifferentDataTypes_ProducesVariableShadow()
        {
            // Arrange
            var variable = new Variable("a", true, Types.GetTypeShim(typeof(bool), true));
            var assignParser = CreateParser<AssignParser>("a <- true", variable);

            // Act
            var result = assignParser.Parse() as Assign;

            // Act
            Assert.That(assignParser.Parser.FindVariable("a"), Is.InstanceOf<ShadowingVariable>());
            assignParser.AssertStackIsEmpty();
        }
    }
}
