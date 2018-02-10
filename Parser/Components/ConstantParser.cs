namespace Parser.Components
{
    using Expressions;
    using SyntaxNodes;
    using System.Text;

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
            if (Parser.TryReadVerbatim(Kind.Literal, out var _, "false"))
            {
                return Constant.False;
            }
            if (Parser.TryReadVerbatim(Kind.Literal, out var _, "null"))
            {
                return Constant.Null;
            }
            if (Parser.TryReadWhile(c => char.IsDigit(((char)c)), Kind.Literal, out var value))
            {
                return new Constant(int.Parse(value.Value), Types.Int);
            }
            Parser.Push();

            if(Parser.TryReadVerbatim(Kind.StringStart, out var stringStartNode, '"'))
            {
                var nodeContents = new StringBuilder();
                var contents = new StringBuilder();
                Position startPos = Parser.CurrentPosition;
                Position lastPos = Parser.CurrentPosition;
                bool isEscape = false;
                while(true)
                {
                    var ch = Parser.ReadChar();
                    
                    if (isEscape)
                    {
                        if (ch == '\\')
                        {
                            nodeContents.Append((char)ch);
                            contents.Append('\\');
                            isEscape = false;
                        }
                        else if (ch == 'n')
                        {
                            nodeContents.Append((char)ch);
                            contents.Append('\n');
                            isEscape = false;
                        }
                        else if (ch == '"')
                        {
                            nodeContents.Append((char)ch);
                            contents.Append('"');
                            isEscape = false;
                        }
                    }
                    else
                    {
                        // Escape
                        if (ch == '\\')
                        {
                            nodeContents.Append((char)ch);
                            isEscape = true;
                        }
                        else if(ch == '"')
                        {
                            Parser.AddSyntaxNode(new SyntaxNode(nodeContents.ToString(), Kind.StringContents, startPos));
                            Parser.AddSyntaxNode(new SyntaxNode("\"", Kind.StringEnd, lastPos));
                            Parser.Merge();
                            return new Constant(contents.ToString(), Types.String);
                        }
                        else
                        {
                            nodeContents.Append((char)ch);
                            contents.Append((char)ch);
                        }
                    }
                    lastPos = Parser.CurrentPosition;
                }
            }
            return Parser.Parse<Call>();
        }
    }
}
