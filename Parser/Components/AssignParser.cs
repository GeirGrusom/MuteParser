namespace Parser.Components
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
            using (var stack = Parser.Push())
            {
                var lhs = Parser.Parse<Member>();
                if (lhs == null)
                {
                    return null;
                }

                var pos = Parser.CurrentPosition;
                if (Parser.TryReadVerbatim(Kind.Operator, out var assignmentOpNode, "<-"))
                {
                    var rhs = Parser.Parse<Binary>();
                    if (rhs != null)
                    {
                        stack.Merge();
                        if (!lhs.IsMutable())
                        {
                            Parser.SyntaxError($"The left hand expression is not assignable", assignmentOpNode.Position);
                        }

                        var result = new Assign(lhs, rhs);
                        if (lhs is Variable variable)
                        {
                            result = CreateAssignment(lhs, assignmentOpNode, rhs, result, variable);
                        }
                        return result;
                    }
                    else
                    {
                        Parser.SyntaxError("Expected expression", pos);
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        private Assign CreateAssignment(Expression lhs, SyntaxNode assignmentOpNode, Expression rhs, Assign result, Variable variable)
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
            else
            {
                if (lhs.Type != rhs.Type)
                {
                    Parser.Shadow(variable, rhs.Type);
                }
            }

            var assigned = new VariableAssignedTrivia(variable);
            Parser.CurrentScope.Trivia.Add(assigned);
            result.Trivia.Add(assigned);
            result = result.WithTrivia(lhs);
            result = result.WithTrivia(rhs);
            return result;
        }
    }
}
