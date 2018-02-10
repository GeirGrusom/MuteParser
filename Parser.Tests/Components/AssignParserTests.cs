namespace Parser.Tests.Components
{
    using System.Linq;

    using SyntaxTrivia;

    using NUnit.Framework;


    [TestFixture]
    public class AssignParserTests
    {
        [Test]
        public void Assign_Parse_Ok()
        {            
            // Arrange
            var parser = new Parser("a <- true");
            parser.PushScopeStack();
            var variable = new global::Parser.Expressions.Variable("a", true, Types.Bool);
            parser.CurrentScope.Add(variable);
            var assignParser = new global::Parser.Components.AssignParser(parser);

            // Act
            var result = assignParser.Parse() as global::Parser.Expressions.Assign;

            // Act
            Assert.That(result.Left, Is.SameAs(variable));
            Assert.That(parser.StackCount, Is.EqualTo(0));            
        }

        [Test]
        public void Assign_Parse_Trivia_VariableAssigned()
        {
            // Arrange
            var parser = new Parser("a <- true");
            parser.PushScopeStack();
            var variable = new global::Parser.Expressions.Variable("a", true, Types.Bool);
            parser.CurrentScope.Add(variable);
            var assignParser = new global::Parser.Components.AssignParser(parser);

            // Act
            var result = assignParser.Parse() as global::Parser.Expressions.Assign;

            // Act
            Assert.That(result.Trivia, Has.One.InstanceOf<VariableAssignedTrivia>());
            var assigned = result.Trivia.OfType<VariableAssignedTrivia>().Single();
            Assert.That(assigned.Variable, Is.SameAs(variable));
            Assert.That(parser.StackCount, Is.EqualTo(0));
        }

        [Test]
        public void Assign_MissingExpression_ReturnsNull()
        {
            // Arrange
            var parser = new Parser("a <- ");
            parser.PushScopeStack();
            var variable = new global::Parser.Expressions.Variable("a", true, Types.Bool);
            parser.CurrentScope.Add(variable);
            var assignParser = new global::Parser.Components.AssignParser(parser);

            // Act
            var result = assignParser.Parse() as global::Parser.Expressions.Assign;

            // Act
            Assert.That(result, Is.Null);
            Assert.That(parser.StackCount, Is.EqualTo(0));
        }

        [Test]
        public void Assign_MissingTarget_ReturnsNull()
        {
            // Arrange
            var parser = new Parser("<- 100");
            parser.PushScopeStack();
            var variable = new global::Parser.Expressions.Variable("a", true, Types.Bool);
            parser.CurrentScope.Add(variable);
            var assignParser = new global::Parser.Components.AssignParser(parser);

            // Act
            var result = assignParser.Parse() as global::Parser.Expressions.Assign;

            // Act
            Assert.That(result, Is.Null);
            Assert.That(parser.StackCount, Is.EqualTo(0));
        }
    }
}
