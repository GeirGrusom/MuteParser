using System;
using System.Collections.Generic;
using System.Text;
using Parser.Expressions;
using Parser.SyntaxNodes;

namespace Parser.Components
{
    public sealed class AndAlsoParser : ParserComponent<AndAlso>
    {
        public AndAlsoParser(Parser parser) : base(parser)
        {
        }

        public override Expression Parse()
        {
            var lhs = Parser.Parse<Equal>();

            if (Parser.TryReadVerbatim(Kind.Operator, out var op, '&'))
            {
                var rhs = Parser.Parse<AndAlso>();
                if (rhs == null)
                {
                    return lhs;
                }
                return new AndAlso(lhs, rhs).WithTrivia(lhs.Trivia).WithTrivia(rhs.Trivia);
            }
            else
            {
                return lhs;
            }
        }
    }
}
