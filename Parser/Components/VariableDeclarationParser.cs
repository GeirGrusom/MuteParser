namespace Parser.Components
{
    using Expressions;
    using global::Parser.SyntaxNodes;
    using SyntaxTrivia;
    public sealed class VariableDeclarationParser : ParserComponent<VariableDeclaration>
    {
        public VariableDeclarationParser(Parser parser) : base(parser)
        {
        }

        public override Expression Parse()
        {
            var pos = Parser.CurrentPosition;
            using (var stack = Parser.Push())
            {

                bool isMutable;

                if (Parser.TryReadVerbatim(Kind.Keyword, out var defNode, "let"))
                {
                    isMutable = false;
                }
                else if (Parser.TryReadVerbatim(Kind.Keyword, out defNode, "var"))
                {
                    isMutable = true;
                }
                else
                {
                    return null;
                }

                if (!Parser.TryReadIdentifier(out var name))
                {
                    Parser.SyntaxError("Let expression: expected identifier");
                    return null;
                }

                TypeShim type;

                if (Parser.TryReadVerbatim(Kind.VariableType, out var typeStartNode, ':'))
                {
                    type = Parser.Parse<DataType>()?.Type;
                }
                else
                {
                    type = null;
                }
                Expression result = null;

                if (Parser.TryReadVerbatim(Kind.Operator, out var assignNode, "<-"))
                {
                    result = Parser.Parse<Binary>();
                    if (result == null)
                    {
                        Parser.SyntaxError("Expected expression");
                        return null;
                    }
                    if (type == null)
                    {
                        type = result.Type;
                    }
                }
                else if (type == null)
                {
                    Parser.SyntaxError("A variable declaration without a type must have an assignment.", name.Position);
                }

                if (result != null && !Types.IsAssignable(type, result.Type))
                {
                    Parser.SyntaxError($"Cannot assign expression of type {result.Type} to {type}.", name.Position);
                }

                stack.Merge();
                var res = new VariableDeclaration(name.Value.ToString(), isMutable, type, result);

                var shadow = Parser.FindVariable(name.Value.ToString());
                if (shadow != null)
                {
                    Parser.SyntaxError($"Cannot shadow variable {name}", name.Position);
                }
                var assigned = new VariableAssignedTrivia(res.Variable);

                res.Trivia.Add(assigned);
                Parser.CurrentScope.Trivia.Add(assigned);

                Parser.CurrentScope.Add(res.Variable);

                if (result != null && type != result.Type)
                {
                    Parser.CurrentScope.Add(new ShadowingVariable(res.Variable, result.Type));
                }
                return res;
            }
            
        }
    }
}
