﻿namespace Parser.Components
{
    using Expressions;
    using global::Parser.SyntaxNodes;
    using SyntaxTrivia;

    public sealed class AssignParser : ParserComponent<Assign>
    {
        public AssignParser(Parser parser) : base(parser)
        {
        }

        public override Expression Parse()
        {
            var start = Parser.CurrentPosition;
            Parser.Push();
            var lhs = Parser.Parse<Member>();
            if (lhs == null)
            {
                Parser.Pop();
                return null;
            }

            var pos = Parser.CurrentPosition;
            if (Parser.TryReadVerbatim(Kind.Operator, out var assignmentOpNode, "<-"))
            {                
                var rhs = Parser.Parse<Binary>();
                if (rhs != null)
                {
                    Parser.Merge();
                    if (!lhs.IsMutable())
                    {
                        Parser.SyntaxError($"The left hand expression is not assignable", assignmentOpNode.Position);
                    }

                    var result = new Assign(lhs, rhs);
                    if (lhs is Variable variable)
                    {
                        if (variable is ShadowingVariable shadowing)
                        {
                            if (!Types.IsAssignable(shadowing.Shadow.Type, rhs.Type))
                            {
                                Parser.SyntaxError($"Cannot assign expression of type {rhs.Type} to {shadowing.Shadow.Type}.", assignmentOpNode.Position);
                            }
                            
                            if (shadowing.Type != rhs.Type)
                            {
                                Parser.Shadow(shadowing.Shadow, rhs.Type);
                            }
                        }
                        else if (!Types.IsAssignable(variable.Type, rhs.Type))
                        {
                            Parser.SyntaxError($"Cannot assign expression of type {rhs.Type} to {variable.Type}.", assignmentOpNode.Position);
                        }

                        var assigned = new VariableAssignedTrivia(variable);
                        Parser.CurrentScope.Trivia.Add(assigned);
                        result.Trivia.Add(assigned);
                        result.Trivia.AddRange(lhs.Trivia);
                        result.Trivia.AddRange(rhs.Trivia);
                    }
                    return result;
                }
                else
                {
                    Parser.SyntaxError("Expected expression", pos);
                    Parser.Pop();
                    return null;
                }
            }
            else
            {
                Parser.Pop();
                return null;
            }
        }
    }
}