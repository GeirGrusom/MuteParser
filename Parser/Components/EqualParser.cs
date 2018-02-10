namespace Parser.Components
{
    using Expressions;
    using global::Parser.SyntaxNodes;
    using SyntaxTrivia;
    public sealed class EqualParser : ParserComponent<Equal>
    {
        public EqualParser(Parser parser) : base(parser)
        {
        }

        public override Expression Parse()
        {
            var lhs = Parser.Parse<Add>();

            var pos = Parser.CurrentPosition;
            if (Parser.TryReadVerbatim(Kind.Operator, out var opNode, '='))
            {
                var rhs = (Expression)Parser.Parse<Equal>();
                var result = new Equal(lhs, rhs);

                void AddVariableIsNullTrivia(Variable variable)
                {
                    if (!variable.Type.Nullable)
                    {
                        Parser.SyntaxError("Comparing non-null to null", opNode.Position);
                    }
                    result.Trivia.Add(new VariableIsNullTrivia(variable));
                }

                if (rhs == Constant.Null && lhs is Variable lhsVariable)
                {
                    AddVariableIsNullTrivia(lhsVariable);
                }

                if (lhs == Constant.Null && rhs is Variable rhsVariable)
                {
                    AddVariableIsNullTrivia(rhsVariable);
                }

                return result;
            }
            else if (Parser.TryReadVerbatim(Kind.Operator, out opNode, "!="))
            {
                var rhs = Parser.Parse<Equal>();

                var result = new NotEqual(lhs, rhs);

                void AddVariableIsNotNullTrivia(Variable variable)
                {
                    if (!variable.Type.Nullable)
                    {
                        Parser.SyntaxError("Comparing non-null to null", opNode.Position);
                    }
                    result.Trivia.Add(new VariableIsNotNullTrivia(variable));
                }


                if (rhs == Constant.Null && lhs is Variable lhsVariable)
                {
                    AddVariableIsNotNullTrivia(lhsVariable);
                }

                if (lhs == Constant.Null && rhs is Variable rhsVariable)
                {
                    AddVariableIsNotNullTrivia(rhsVariable);
                }
                return result;
            }
            else
            {
                return lhs;
            }
        }
    }
}
