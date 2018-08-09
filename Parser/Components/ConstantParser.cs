namespace Parser.Components
{
    using Expressions;
    using SyntaxNodes;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public sealed class ConstantParser : ParserComponent<Constant>
    {
        public ConstantParser(Parser parser) : base(parser)
        {
        }

        public override Expression Parse()
        {
            using (var stack = Parser.Push())
            {
                if (Parser.TryReadVerbatim(Kind.Punctuation, out var parensStartNode, '('))
                {
                    var names = new List<string>();
                    var tupleExpressions = new List<Expression>();

                    do
                    {
                        string name = null;
                        using (var labelStack = Parser.Push())
                        {
                            if (Parser.TryReadIdentifier(out var keyName) && Parser.TryReadVerbatim(Kind.Operator, out var def, ':'))
                            {
                                name = keyName.Value.ToString();
                                labelStack.Merge();
                            }
                        }
                        var inner = Parser.Parse<Binary>();

                        if (inner == null)
                        {
                            Parser.SyntaxError("Expected expression");
                        }
                        tupleExpressions.Add(inner);
                        names.Add(name);
                    }
                    while (Parser.TryReadVerbatim(Kind.TupleSeparator, out var tupleSep, ','));


                    if (!Parser.TryReadVerbatim(Kind.Punctuation, out var parensEndNode, ')'))
                    {
                        Parser.SyntaxError("Expected ')'");
                    }

                    stack.Merge();
                    if (tupleExpressions.Count == 1)
                    {
                        return tupleExpressions[0];
                    }
                    return new Expressions.Tuple(tupleExpressions.ToArray());
                }

                if (Parser.TryReadVerbatim(Kind.Literal, out var _, "true"))
                {
                    stack.Merge();
                    return Constant.True;
                }
                if (Parser.TryReadVerbatim(Kind.Literal, out var _, "false"))
                {
                    stack.Merge();
                    return Constant.False;
                }
                if (Parser.TryReadVerbatim(Kind.Literal, out var _, "null"))
                {
                    stack.Merge();
                    return Constant.Null;
                }
                if (Parser.TryReadWhile(c => char.IsDigit(((char)c)), Kind.Literal, out var value))
                {
                    stack.Merge();
                    return new Constant(int.Parse(value.Value.Span), Types.Int);
                }
                if (Parser.TryReadVerbatim(Kind.StringStart, out var stringStartNode, '"'))
                {
                    var contents = new StringBuilder();
                    Position startPos = Parser.CurrentPosition;
                    Position lastPos = Parser.CurrentPosition;
                    bool isEscape = false;
                    while (true)
                    {
                        var ch = Parser.ReadChar();

                        if (isEscape)
                        {
                            if (ch == '\\')
                            {
                                contents.Append('\\');
                                isEscape = false;
                            }
                            else if (ch == 'n')
                            {
                                contents.Append('\n');
                                isEscape = false;
                            }
                            else if (ch == '"')
                            {
                                contents.Append('"');
                                isEscape = false;
                            }
                        }
                        else
                        {
                            // Escape
                            if (ch == '\\')
                            {
                                isEscape = true;
                            }
                            else if (ch == '"')
                            {
                                break;
                            }
                            else
                            {
                                contents.Append((char)ch);
                            }
                        }
                        lastPos = Parser.CurrentPosition;
                    }
                    Parser.AddSyntaxNode(new SyntaxNode(this.Parser.Source.AsMemory(startPos.Value, lastPos.Value - startPos.Value - 1), Kind.StringContents, startPos));
                    Parser.AddSyntaxNode(new SyntaxNode(this.Parser.Source.AsMemory(lastPos.Value, 1), Kind.StringEnd, lastPos));
                    stack.Merge();
                    return new Constant(contents.ToString(), Types.String);
                }

                if (Parser.TryReadVerbatim(Kind.StartArray, out var arrStart, '['))
                {
                    var elements = new List<Expression>();
                    while (true)
                    {
                        var item = Parser.Parse<Binary>();
                        elements.Add(item);
                        if (Parser.TryReadVerbatim(Kind.EndArray, out var endNode, ']'))
                        {
                            break;
                        }

                        if (!Parser.TryReadVerbatim(Kind.ArraySeparator, out var sepNode, ','))
                        {
                            Parser.SyntaxError("Expected ',' or ']'");
                        }
                    }

                    TypeShim resultType;

                    if (Parser.TryReadVerbatim(Kind.Operator, out var typeNode, ':'))
                    {
                        resultType = Parser.Parse<DataType>().Type;
                    }
                    else
                    {
                        resultType = new ArrayTypeShim(new[] { 1 }, elements[0].Type, elements.Any(x => x.Type == Types.Null));
                    }
                    stack.Merge();
                    return new Expressions.Array(elements, (ArrayTypeShim)resultType);
                }
            }
            return Parser.Parse<Call>();
        }
    }
}


