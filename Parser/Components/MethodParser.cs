﻿using Parser.Expressions;
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
            Parser.Push();
            Parser.PushScopeStack();
            if (!Parser.TryReadIdentifier(out var methodName))
            {
                Parser.PopScopeStack();
                Parser.Pop();
                return null;
            }
            if (!Parser.TryReadVerbatim(Kind.MethodArgumentsStart, out var argStartNode, '('))
            {
                Parser.PopScopeStack();
                Parser.Pop();
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
                    Parser.Pop();
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
                    Parser.Pop();
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
                Parser.Pop();
                return null;
            }
            Parser.Merge();
            return new Method(returnType ?? block.Type, methodName.Value, block, Parser.PopScopeStack(), varDecl.ToArray());
        }
    }
}
