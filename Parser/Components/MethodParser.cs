using Parser.Expressions;
using Parser.SyntaxNodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Components
{
    public sealed class MethodParser : ParserComponent<Method>
    {
        public MethodParser(Parser parser) : base(parser)
        {
            
        }

        public override Expression Parse()
        {
            using (var stack = Parser.Push())
            {
                Parser.PushScopeStack();
                if (!Parser.TryReadIdentifier(out var methodName))
                {
                    Parser.PopScopeStack();
                    return null;
                }
                if (!Parser.TryReadVerbatim(Kind.MethodArgumentsStart, out var argStartNode, '('))
                {
                    Parser.PopScopeStack();
                    return null;
                }

                var varDecl = new List<Variable>();
                while (true)
                {
                    var start = Parser.CurrentPosition;
                    var par = (Parameter)Parser.Parse<Parameter>();

                    if (par == null)
                    {
                        if (Parser.TryReadVerbatim(Kind.MethodArgumentsEnd, out var argEndNode, ')'))
                        {
                            break;
                        }
                        Parser.CurrentPosition = start;
                        Parser.SyntaxError("Expected parameter");
                        Parser.PopScopeStack();
                        return null;
                    }
                    varDecl.Add(par);
                    Parser.CurrentScope.Add(par);
                    if (Parser.TryReadVerbatim(Kind.MethodArgumentsSeparator, out var argSepNode, ','))
                    {
                        continue;
                    }
                    else if (Parser.TryReadVerbatim(Kind.MethodArgumentsEnd, out var argEndNode, ')'))
                    {
                        break;
                    }
                    else
                    {
                        Parser.PopScopeStack();
                        return null;
                    }
                }

                TypeShim returnType = null;
                if (Parser.TryReadVerbatim(Kind.TypeResult, out var resultTypeNode, "=>"))
                {
                    returnType = Parser.Parse<DataType>()?.Type;
                }

                var block = Parser.Parse<Block>();

                if (block == null)
                {
                    Parser.PopScopeStack();
                    return null;
                }
                stack.Merge();
                var result = new Method(returnType ?? block.Type, methodName.Value.ToString(), block, Parser.PopScopeStack(), varDecl.ToArray());
                Parser.CurrentScope.Add(result);
                return result;
            }
        }
    }
}
