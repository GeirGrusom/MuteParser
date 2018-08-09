using Parser.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Components
{
    public sealed class AddParser : ParserComponent<Add>
    {

        public AddParser(Parser parser) : base(parser)
        {
        }

        public override Expression Parse()
        {
            var lhs = Parser.Parse<Multiply>();

            if (Parser.TryReadVerbatim(SyntaxNodes.Kind.Operator, out var op, '+', '-'))
            {
                var rhs = Parser.Parse<Add>();

                ReadOnlySpan<char> plusSpan = stackalloc char[] { '+' };
                ReadOnlySpan<char> minusSpan = stackalloc char[] { '-' };

                if (op.Value.Span.Equals(plusSpan, StringComparison.Ordinal))
                {
                    return new Add(lhs, rhs, lhs.Type);
                }
                if (op.Value.Span.Equals(minusSpan, StringComparison.Ordinal))
                {
                    return new Subtract(lhs, rhs, lhs.Type);
                }

                throw new NotImplementedException();
            }
            else
            {
                return lhs;
            }
        }
    }
}
