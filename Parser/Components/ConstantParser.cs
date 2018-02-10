namespace Parser.Components
{
    using System.Linq;
    using Expressions;
    using global::Parser.SyntaxNodes;
    using SyntaxTrivia;
    public sealed class ConstantParser : ParserComponent<Constant>
    {
        public ConstantParser(Parser parser) : base(parser)
        {
        }

        public override Expression Parse()
        {
            Parser.Push();
            if(Parser.TryReadVerbatim(Kind.Punctuation, out var parensStartNode, '('))
            {
                var inner = Parser.Parse<Binary>();

                bool success = true;
                if(inner == null)
                {
                    Parser.SyntaxError("Expected expression");
                    Parser.Pop();
                    success = false;
                }

                if(!Parser.TryReadVerbatim(Kind.Punctuation, out var parensEndNode, ')'))
                {
                    Parser.SyntaxError("Expected ')'");
                    Parser.Pop();
                    success = false;
                }
                if (success)
                {
                    Parser.Merge();
                }
                return inner;
            }
            else
            {
                Parser.Pop();
            }

            if (Parser.TryReadVerbatim(Kind.Literal, out var _, "true"))
            {
                return Constant.True;
            }
            else if (Parser.TryReadVerbatim(Kind.Literal, out var _, "false"))
            {
                return Constant.False;
            }
            else if (Parser.TryReadVerbatim(Kind.Literal, out var _, "null"))
            {
                return Constant.Null;
            }
            if (Parser.TryReadWhile(c => char.IsDigit(((char)c)), Kind.Literal, out var value))
            {
                return new Constant(int.Parse(value.Value), Types.Int);
            }
            return Parser.Parse<Member>();
        }
    }
}
