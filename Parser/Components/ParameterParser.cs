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
            Parser.Push();
            if (!Parser.TryReadIdentifier(out var name))
            {
                Parser.Pop();
                return null;
            }

            if (Parser.TryReadVerbatim(Kind.VariableType, out var paramTypeNode, ':'))
            {
                var typeExpr = Parser.Parse<DataType>();
                if (typeExpr == null)
                {
                    Parser.Pop();
                    return null;
                }
                Parser.Merge();
                return new Parameter(name.Value, false, typeExpr.Type);
            }
            else
            {
                Parser.Pop();
                return null;
            }
        }
    }
}
