using System;
using System.Collections.Generic;
using System.Text;
using Parser.Expressions;
using Parser.SyntaxNodes;
using Parser.SyntaxTrivia;

namespace Parser.Components
{
    public sealed class UnaryParser : ParserComponent<Unary>
    {
        public UnaryParser(Parser parser) : base(parser)
        {
        }

        public override Expression Parse()
        {
            Parser.Push();
            if (Parser.TryReadVerbatim(Kind.Operator, out var parensStart, '('))
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
            if (Parser.TryReadVerbatim(Kind.Operator, out var opNode, '+', '-', '!'))
            {
                var operandPos = Parser.CurrentPosition;
                var operand = Parser.Parse<Unary>();

                if(operand is null)
                {
                    Parser.SyntaxError("Expected expression", operandPos);
                }

                Parser.Merge();
                switch (opNode.Value)
                {
                    case "+":
                        return new Positive(operand);
                    case "-":
                        return new Negate(operand);
                    case "!":
                        var result = new Not(operand);
                        foreach(var trivia in operand.Trivia)
                        {
                            if(trivia is VariableIsNotNullTrivia notNullTrivia )
                            {
                                result.Trivia.Add(new VariableIsNullTrivia(notNullTrivia.Variable));
                            }
                            else if(trivia is VariableIsNullTrivia nullTrivia)
                            {
                                result.Trivia.Add(new VariableIsNotNullTrivia(nullTrivia.Variable));
                            }
                        }
                        return result;
                }
                throw new NotSupportedException();
            }
            Parser.Pop();
            var pos = Parser.CurrentPosition;
            var lhs = Parser.Parse<Constant>();

            if (Parser.TryReadVerbatim(Kind.Operator, out var opNotNull, "!!"))
            {
                if (lhs is null)
                {
                    Parser.SyntaxError("Expected expression", pos);
                }

                Parser.Merge();
                if (!lhs.Type.Nullable)
                {
                    Parser.SyntaxError("Operand is already non-nullable", opNotNull.Position);
                }
                return new NotNull(lhs);
            }
            return lhs;
        }
    }
}
