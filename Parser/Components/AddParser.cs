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
                switch (op.Value)
                {
                    case "+":
                        return new Add(lhs, rhs, lhs.Type);
                    case "-":
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
