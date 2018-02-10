using Parser.Expressions;
using Parser.SyntaxNodes;
using System;
using System.Collections.Generic;
using System.Text;

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
                switch (op.Value)
                {
                    case "*":
                        return new Multiply(lhs, rhs, lhs.Type);
                    case "/":
                        return new Divide(lhs, rhs, lhs.Type);
                    case "%":
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
