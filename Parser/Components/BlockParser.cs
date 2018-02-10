using System;
using System.Collections.Generic;
using System.Text;
using Parser.Expressions;
using Parser.SyntaxNodes;

namespace Parser.Components
{
    public sealed class BlockParser : ParserComponent<Block>
    {
        public BlockParser(Parser parser) : base(parser)
        {
        }

        public override Expression Parse()
        {
            Parser.Push();
            if (!Parser.TryReadVerbatim(Kind.Punctuation, out var startBlock, '{'))
            {
                Parser.Pop();
                return null;
            }

            List<Expression> statements = new List<Expression>();
            while (true)
            {
                var nextStatement = ParseStatement();
                if (nextStatement == null)
                {
                    break;
                }
                statements.Add(nextStatement);
            }

            if (!Parser.TryReadVerbatim(Kind.Punctuation, out var endBlock, '}'))
            {
                Parser.SyntaxError("Expected '}'");
                Parser.Pop();
                return null;
            }

            Parser.Merge();
            var result = new Block(Parser.CurrentScope, statements.ToArray());
            foreach (var exp in statements)
            {
                result.Trivia.AddRange(exp.Trivia);
            }
            return result;
        }

        private Expression ParseStatement()
        {
            return Parser.Parse<VariableDeclaration>() ?? Parser.Parse<Assign>() ?? Parser.Parse<If>() ?? Parser.Parse<Method>() ?? Parser.Parse<Using>();
        }

    }
}
