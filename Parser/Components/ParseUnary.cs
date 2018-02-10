using System;
using System.Collections.Generic;
using System.Text;
using Parser.Expressions;
using Parser.SyntaxNodes;

namespace Parser.Components
{
    public sealed class ParseUnary : ParserComponent<Unary>
    {
        public ParseUnary(Parser parser) : base(parser)
        {
        }

        public override Expression Parse()
        {
            Parser.Push();
            if(Parser.TryReadVerbatim(Kind.Operator, out var parensStart, '('))
            {
                var operand = Parser.Parse<Binary>();
                if (!Parser.TryReadVerbatim(Kind.Operator, out var parensEnd, ')'))
                {
                    Parser.SyntaxError("Expected ')'");
                    Parser.Pop();
                    return null;
                }
                Parser.Merge();
                return operand;
            }
            if(Parser.TryReadVerbatim(Kind.Operator, out var notNullNode, "!?"))
            {
                var operand = Parser.Parse<Unary>();
                Parser.Merge();
                if(!operand.Type.Nullable)
                {
                    Parser.SyntaxError("Operand is already non-nullable", notNullNode.Position);
                }
                return new NotNull(operand);
            }
            if(Parser.TryReadVerbatim(Kind.Operator, out var opNode, '+', '-', '!'))
            {
                var operand = Parser.Parse<Unary>();
                Parser.Merge();
                switch(opNode.Value)
                {
                    case "+":
                        return new Positive(operand);
                    case "-":
                        return new Negate(operand);
                    case "!":
                        return new Not(operand);
                }
                throw new NotSupportedException();
            }
            Parser.Pop();
            return Parser.Parse<Constant>();
        }
    }
}
