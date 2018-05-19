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
            using (var stack = Parser.Push())
            {
                if (Parser.TryReadVerbatim(Kind.Operator, out var opNode, '+', '-', '!'))
                {
                    var operandPos = Parser.CurrentPosition;
                    var operand = Parser.Parse<Unary>();

                    if (operand is null)
                    {
                        Parser.SyntaxError("Expected expression", operandPos);
                    }

                    stack.Merge();
                    switch (opNode.Value)
                    {
                        case "+":
                            return new Plus(operand);
                        case "-":
                            return new Minus(operand);
                        case "!":
                            var result = new Not(operand);
                            foreach (var trivia in operand.Trivia)
                            {
                                if (trivia is VariableIsNotNullTrivia notNullTrivia)
                                {
                                    result.Trivia.Add(new VariableIsNullTrivia(notNullTrivia.Variable));
                                }
                                else if (trivia is VariableIsNullTrivia nullTrivia)
                                {
                                    result.Trivia.Add(new VariableIsNotNullTrivia(nullTrivia.Variable));
                                }
                            }
                            return result;
                    }
                    throw new NotSupportedException();
                }

                var pos = Parser.CurrentPosition;
                var lhs = Parser.Parse<Constant>();

                if (Parser.TryReadVerbatim(Kind.Operator, out var opNotNull, "!!"))
                {
                    if (lhs is null)
                    {
                        Parser.SyntaxError("Expected expression", pos);
                    }

                    stack.Merge();
                    if (!lhs.Type.Nullable)
                    {
                        Parser.SyntaxError("Operand is already non-nullable", opNotNull.Position);
                    }
                    return new NotNull(lhs);
                }

                else if (Parser.TryReadVerbatim(Kind.StartArray, out var opArrayStart, '['))
                {
                    if (!(lhs.Type is ArrayTypeShim arrayTypeShim))
                    {
                        Parser.SyntaxError("The left hand expression is not a array type.");
                    }
                    else
                    {
                        var indices = new List<Expression>();
                        while (true)
                        {
                            var operand = Parser.Parse<Binary>();

                            indices.Add(operand);

                            if (Parser.TryReadVerbatim(Kind.EndArray, out var opArrayEnd, ']'))
                            {
                                break;
                            }
                            if (!Parser.TryReadVerbatim(Kind.ArraySeparator, out var sep, ','))
                            {
                                Parser.SyntaxError("Expected ',' or ']'");
                            }
                        }

                        if (indices.Count != arrayTypeShim.Dimensions.Length)
                        {
                            Parser.SyntaxError($"The array has {arrayTypeShim.Dimensions.Length} dimensions, but {indices.Count} were specified.");
                        }

                        return new IndexDereference(lhs, indices);
                    }
                }
                stack.Merge(); 
                return lhs;
            }
        }
    }
}
