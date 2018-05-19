using Parser.Expressions;
using Parser.SyntaxNodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Components
{
    public class ParameterParser : ParserComponent<Parameter>
    {
        public ParameterParser(Parser parser) : base(parser)
        {
        }

        public override Expression Parse()
        {
            using (var stack = Parser.Push())
            {
                if (!Parser.TryReadIdentifier(out var name))
                {
                    return null;
                }

                if (Parser.TryReadVerbatim(Kind.VariableType, out var paramTypeNode, ':'))
                {
                    var typeExpr = Parser.Parse<DataType>();
                    if (typeExpr == null)
                    {
                        return null;
                    }
                    stack.Merge();
                    return new Parameter(name.Value, false, typeExpr.Type);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
