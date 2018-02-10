using NUnit.Framework;
using Parser.Components;
using Parser.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Tests.Components
{
    [TestFixture]
    public class BlockParserTests
    {
        [Test]
        public void Parse_Empty_ReturnsEmptyBlock()
        {
            // Arrange
            var parser = new Parser("{ }");
            var blockParser = new BlockParser(parser);
            parser.PushScopeStack();

            // Act

            var result = blockParser.Parse();

            // Assert
            Assert.That(result, Is.InstanceOf<Block>().With.Property(nameof(Block.Statements)).Empty);
            Assert.That(parser.StackCount, Is.EqualTo(0));
        }

        [Test]
        public void Parse_Method_ReturnsBlockWithMethod()
        {
            // Arrange
            var parser = new Parser("{ main() { } }");
            var blockParser = new BlockParser(parser);
            parser.PushScopeStack();

            // Act

            var result = blockParser.Parse() as Block;

            // Assert
            Assert.That(result.Statements, Has.One.InstanceOf<Method>());
            Assert.That(parser.StackCount, Is.EqualTo(0));
        }

        [Test]
        public void Parse_NotBlock_ReturnsNull()
        {
            // Arrange
            var parser = new Parser("foo");
            var blockParser = new BlockParser(parser);
            parser.PushScopeStack();

            // Act

            var result = blockParser.Parse();

            // Assert
            Assert.That(result, Is.Null);
            Assert.That(parser.StackCount, Is.EqualTo(0));
        }
    }
}
