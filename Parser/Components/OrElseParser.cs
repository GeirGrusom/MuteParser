using Parser.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Components
{
    public sealed class OrElseParser : ParserComponent<OrElse>
    {
        public OrElseParser(Parser parser) : base(parser)
        {
        }

        public override Expression Parse()
        {
            var lhs = Parser.Parse<AndAlso>();

            if (Parser.TryReadVerbatim(SyntaxNodes.Kind.Operator, out var op, "|"))
            {
                var rhs = Parser.Parse<OrElse>();
                return new OrElse(lhs, rhs);
            }
            else
            {
                return lhs;
            }
        }
    }
}
