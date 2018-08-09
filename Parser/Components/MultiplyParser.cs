using Parser.Expressions;
using Parser.SyntaxNodes;
using System;

namespace Parser.Components
{
    public sealed class MultiplyParser : ParserComponent<Multiply>
    {


        public MultiplyParser(Parser parser) : base(parser)
        {
        }

        public override Expression Parse()
        {
            var lhs = Parser.Parse<Unary>();

            if (Parser.TryReadVerbatim(Kind.Operator, out var op, '*', '/', '%'))
            {
                var rhs = Parser.Parse<Multiply>();
                if (rhs == null)
                {
                    return lhs;
                }

                ReadOnlySpan<char> mulSpan = stackalloc char[] { '*' };

                if (op.Value.Span.Equals(mulSpan, StringComparison.Ordinal))
                {
                    return new Multiply(lhs, rhs, lhs.Type);

                }

                ReadOnlySpan<char> divSpan = stackalloc char[] { '/' };

                if (op.Value.Span.Equals(divSpan, StringComparison.Ordinal))
                {
                    return new Divide(lhs, rhs, lhs.Type);
                }

                ReadOnlySpan<char> remSpan = stackalloc char[] { '%' };

                if (op.Value.Span.Equals(remSpan, StringComparison.Ordinal))
                { 
                        return new Remainder(lhs, rhs, lhs.Type);
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
